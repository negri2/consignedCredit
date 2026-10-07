using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Infrastructure.Messaging.RabbitMq
{
    public sealed class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string User { get; set; } = "guest";
        public string Password { get; set; } = "guest";

        public string Exchange { get; set; } = "consigned-credit";
        public string Queue { get; set; } = "proposal-processing";
        public string RoutingKey { get; set; } = "proposal.created";
    }
}
