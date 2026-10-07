using ConsignedCredit.Application.Abstractions.Messaging;
using ConsignedCredit.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsignedCredit.Infrastructure.Messaging.Outbox
{
    public sealed class Outbox : IOutbox
    {
        private readonly ConsignedCreditDbContext _context;

        public Outbox(ConsignedCreditDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync<T>(
            T message,
            CancellationToken cancellationToken = default)
        {
            var outboxMessage = new OutboxMessage(
                typeof(T).Name,
                JsonSerializer.Serialize(message));

            await _context.Set<OutboxMessage>()
                .AddAsync(outboxMessage, cancellationToken);
        }
    }
}
