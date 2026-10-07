using ConsignedCredit.Application.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Infrastructure.ExternalServices
{
    public sealed class SimulationValidationService
     : ISimulationValidationService
    {
        public async Task<int> GetScoreAsync(
            Guid proposalId,
            CancellationToken cancellationToken = default)
        {
            // Simulates an external HTTP service call.
            await Task.Delay(
                TimeSpan.FromMilliseconds(200),
                cancellationToken);

            return 8;
        }
    }
}
