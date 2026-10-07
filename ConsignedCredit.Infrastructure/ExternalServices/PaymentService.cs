using ConsignedCredit.Application.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Infrastructure.ExternalServices
{
    public sealed class PaymentService : IPaymentService
    {
        public async Task PayAsync(
            Guid proposalId,
            CancellationToken cancellationToken = default)
        {
            // Simulates an external service call.
            await Task.Delay(
                TimeSpan.FromMilliseconds(200),
                cancellationToken);
        }
    }
}
