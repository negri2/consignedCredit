using ConsignedCredit.Application.Abstractions.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Infrastructure.Messaging.RabbitMq
{
    public sealed class RabbitMqMessagePublisher : IMessagePublisher, IAsyncDisposable
    {
        private readonly RabbitMqOptions _options;
        private readonly SemaphoreSlim _lock = new(1, 1);

        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMqMessagePublisher(
            IOptions<RabbitMqOptions> options)
        {
            _options = options.Value;
        }

        public async Task PublishAsync(
            string type,
            string payload,
            CancellationToken cancellationToken = default)
        {
            await EnsureConnectedAsync(cancellationToken);

            var body = Encoding.UTF8.GetBytes(payload);

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                Type = type,
                Persistent = true
            };

            await _channel!.BasicPublishAsync(
                exchange: _options.Exchange,
                routingKey: _options.RoutingKey,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }

        private async Task EnsureConnectedAsync(
            CancellationToken cancellationToken)
        {
            if (_connection?.IsOpen == true &&
                _channel?.IsOpen == true)
            {
                return;
            }

            await _lock.WaitAsync(cancellationToken);

            try
            {
                if (_connection?.IsOpen == true &&
                    _channel?.IsOpen == true)
                {
                    return;
                }

                var factory = new ConnectionFactory
                {
                    HostName = _options.Host,
                    Port = _options.Port,
                    UserName = _options.User,
                    Password = _options.Password,
                    AutomaticRecoveryEnabled = true,
                    ClientProvidedName = "consigned-credit-api"
                };

                _connection = await factory.CreateConnectionAsync(
                    cancellationToken);

                var channelOptions = new CreateChannelOptions(
                    publisherConfirmationsEnabled: true,
                    publisherConfirmationTrackingEnabled: true);

                _channel = await _connection.CreateChannelAsync(
                    channelOptions,
                    cancellationToken);

                await _channel.ExchangeDeclareAsync(
                    exchange: _options.Exchange,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: cancellationToken);

                await _channel.QueueDeclareAsync(
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
            }
            finally
            {
                _lock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel is not null)
                await _channel.DisposeAsync();

            if (_connection is not null)
                await _connection.DisposeAsync();

            _lock.Dispose();
        }
    }
}
