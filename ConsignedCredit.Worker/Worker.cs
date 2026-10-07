using ConsignedCredit.Application.Proposals.Events;
using ConsignedCredit.Application.Proposals.Process;
using ConsignedCredit.Infrastructure.Messaging.RabbitMq;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ConsignedCredit.Worker;

public sealed class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<Worker> _logger;
    private const string RetryHeader = "x-retry-count";

    private IConnection? _connection;
    private IChannel? _channel;

    public Worker(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.User,
            Password = _options.Password,
            AutomaticRecoveryEnabled = true,
            ClientProvidedName = "consigned-credit-worker"
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(
            cancellationToken: stoppingToken);

        await ConfigureQueuesAsync(stoppingToken);

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            await HandleMessageAsync(args, stoppingToken);
        };

        await _channel.BasicConsumeAsync(
            queue: _options.Queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }

    private async Task HandleMessageAsync(
        BasicDeliverEventArgs args,
        CancellationToken cancellationToken)
    {
        try
        {
            var payload = Encoding.UTF8.GetString(args.Body.Span);

            var message =
                JsonSerializer.Deserialize<ProposalCreatedEvent>(payload);

            if (message is null)
                throw new InvalidOperationException(
                    "Invalid ProposalCreatedEvent payload.");

            using var scope = _scopeFactory.CreateScope();

            var useCase = scope.ServiceProvider
                .GetRequiredService<ProcessProposalUseCase>();

            await useCase.ExecuteAsync(
                message.ProposalId,
                cancellationToken);

            await _channel!.BasicAckAsync(
                args.DeliveryTag,
                multiple: false,
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error processing RabbitMQ message.");

            await HandleFailedMessageAsync(
                args,
                cancellationToken);
        }
    }

    private async Task ConfigureQueuesAsync(
        CancellationToken cancellationToken)
    {
        await _channel!.ExchangeDeclareAsync(
            exchange: _options.Exchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        // Main queue
        await _channel!.QueueDeclareAsync(
            queue: _options.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await _channel.QueueBindAsync(
            queue: _options.Queue,
            exchange: _options.Exchange,
            routingKey: _options.RoutingKey,
            cancellationToken: cancellationToken);

        // Retry queue:
        // after the TTL expires, RabbitMQ sends the message
        // back to the main exchange/routing key.
        var retryArguments = new Dictionary<string, object?>
        {
            ["x-message-ttl"] = _options.RetryDelayMilliseconds,
            ["x-dead-letter-exchange"] = _options.Exchange,
            ["x-dead-letter-routing-key"] = _options.RoutingKey
        };

        await _channel.QueueDeclareAsync(
            queue: _options.RetryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: retryArguments,
            cancellationToken: cancellationToken);

        // Messages that exceeded the retry limit stay here
        // for manual inspection/reprocessing.
        await _channel.QueueDeclareAsync(
            queue: _options.DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);
    }

   

    private async Task HandleFailedMessageAsync(
        BasicDeliverEventArgs args,
        CancellationToken cancellationToken)
    {
        var retryCount = GetRetryCount(args.BasicProperties);

        if (retryCount < _options.MaxRetries)
        {
            await PublishToRetryQueueAsync(
                args,
                retryCount + 1,
                cancellationToken);

            _logger.LogWarning(
                "Message {DeliveryTag} sent to retry queue. Retry {RetryCount}/{MaxRetries}.",
                args.DeliveryTag,
                retryCount + 1,
                _options.MaxRetries);
        }
        else
        {
            await PublishToDeadLetterQueueAsync(
                args,
                cancellationToken);

            _logger.LogError(
                "Message {DeliveryTag} exceeded retry limit and was sent to DLQ.",
                args.DeliveryTag);
        }

        await _channel!.BasicAckAsync(
            args.DeliveryTag,
            multiple: false,
            cancellationToken);
    }

    private async Task PublishToRetryQueueAsync(
        BasicDeliverEventArgs args,
        int retryCount,
        CancellationToken cancellationToken)
    {
        var properties = new BasicProperties
        {
            ContentType = args.BasicProperties.ContentType,
            Type = args.BasicProperties.Type,
            Persistent = true,
            Headers = CopyHeaders(args.BasicProperties.Headers)
        };

        properties.Headers[RetryHeader] = retryCount;

        await _channel!.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _options.RetryQueue,
            mandatory: true,
            basicProperties: properties,
            body: args.Body,
            cancellationToken: cancellationToken);
    }

    private async Task PublishToDeadLetterQueueAsync(
        BasicDeliverEventArgs args,
        CancellationToken cancellationToken)
    {
        var properties = new BasicProperties
        {
            ContentType = args.BasicProperties.ContentType,
            Type = args.BasicProperties.Type,
            Persistent = true,
            Headers = CopyHeaders(args.BasicProperties.Headers)
        };

        await _channel!.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _options.DeadLetterQueue,
            mandatory: true,
            basicProperties: properties,
            body: args.Body,
            cancellationToken: cancellationToken);
    }

    private static int GetRetryCount(
       IReadOnlyBasicProperties properties)
    {
        if (properties.Headers is null ||
            !properties.Headers.TryGetValue(
                RetryHeader,
                out var value))
        {
            return 0;
        }

        return value switch
        {
            int intValue => intValue,
            long longValue => (int)longValue,
            _ => 0
        };
    }

    private static IDictionary<string, object?> CopyHeaders(
    IDictionary<string, object?>? headers)
    {
        return headers is null
            ? new Dictionary<string, object?>()
            : new Dictionary<string, object?>(headers);
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        if (_channel is not null)
            await _channel.DisposeAsync();

        if (_connection is not null)
            await _connection.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }
}
