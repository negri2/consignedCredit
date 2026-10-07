using ConsignedCredit.Application.Abstractions.Messaging;
using ConsignedCredit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Infrastructure.Messaging.Outbox
{
    public sealed class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMessagePublisher _publisher;
        private readonly ILogger<OutboxProcessor> _logger;

        public OutboxProcessor(
            IServiceScopeFactory scopeFactory,
            IMessagePublisher publisher,
            ILogger<OutboxProcessor> logger)
        {
            _scopeFactory = scopeFactory;
            _publisher = publisher;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessMessagesAsync(stoppingToken);
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Error processing outbox messages.");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
        }

        private async Task ProcessMessagesAsync(
            CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var context =
                scope.ServiceProvider
                    .GetRequiredService<ConsignedCreditDbContext>();

            var messages = await context.OutboxMessages
                .Where(x => x.ProcessedAt == null)
                .OrderBy(x => x.CreatedAt)
                .Take(20)
                .ToListAsync(cancellationToken);

            foreach (var message in messages)
            {
                try
                {
                    await _publisher.PublishAsync(
                        message.Type,
                        message.Payload,
                        cancellationToken);

                    message.MarkAsProcessed();

                    await context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Outbox message {MessageId} published successfully.",
                        message.Id);
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Failed to publish outbox message {MessageId}.",
                        message.Id);
                }
            }
        }
    }
}
