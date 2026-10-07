using ConsignedCredit.Application.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Infrastructure.ExternalServices
{
    public sealed class FraudCheckService : IFraudCheckService
    {
        private static readonly HashSet<string> FraudulentCpfs =
        [
            "11111111111",
            "99999999999"
        ];

        public Task<bool> IsFraudulentAsync(
            string cpf,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(FraudulentCpfs.Contains(cpf));
        }
    }
}
