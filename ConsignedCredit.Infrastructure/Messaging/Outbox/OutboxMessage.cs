using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Infrastructure.Messaging.Outbox
{
    public sealed class OutboxMessage
    {
        public Guid Id { get; private set; }
        public string Type { get; private set; }
        public string Payload { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }

        private OutboxMessage()
        {
        }

        public OutboxMessage(
            string type,
            string payload)
        {
            Id = Guid.NewGuid();
            Type = type;
            Payload = payload;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsProcessed()
        {
            ProcessedAt = DateTime.UtcNow;
        }
    }
}
