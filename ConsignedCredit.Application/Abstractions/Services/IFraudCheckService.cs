using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Application.Abstractions.Services
{
    public interface IFraudCheckService
    {
        Task<bool> IsFraudulentAsync(
            string cpf,
            CancellationToken cancellationToken = default);
    }
}
