using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Application.Abstractions.Services
{
    public interface IAgentService
    {
        Task<bool> IsActiveAsync(
            Guid agentId,
            CancellationToken cancellationToken = default);
    }
}
