using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Application.Abstractions.Services
{
    public interface IInssRegistrationService
    {
        Task RegisterAsync(
            Guid proposalId,
            CancellationToken cancellationToken = default);
    }
}
