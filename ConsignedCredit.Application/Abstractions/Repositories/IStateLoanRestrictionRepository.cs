using ConsignedCredit.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Application.Abstractions.Repositories
{
    public interface IStateLoanRestrictionRepository
    {
        Task<StateLoanRestriction?> GetByStateAsync(
            string state,
            CancellationToken cancellationToken = default);
    }
}
