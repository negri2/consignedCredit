using ConsignedCredit.Application.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Infrastructure.ExternalServices
{
    public sealed class AgentService : IAgentService
    {
        public Task<bool> IsActiveAsync(
            Guid agentId,
            CancellationToken cancellationToken = default)
        {
            // Simulates an external agent validation service.
            return Task.FromResult(agentId != Guid.Empty);
        }
    }
}
