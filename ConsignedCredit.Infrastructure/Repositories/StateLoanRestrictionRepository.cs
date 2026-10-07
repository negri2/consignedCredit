using ConsignedCredit.Application.Abstractions.Repositories;
using ConsignedCredit.Domain.Entities;
using ConsignedCredit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Infrastructure.Repositories
{
    public sealed class StateLoanRestrictionRepository
    : IStateLoanRestrictionRepository
    {
        private readonly ConsignedCreditDbContext _context;

        public StateLoanRestrictionRepository(
            ConsignedCreditDbContext context)
        {
            _context = context;
        }

        public Task<StateLoanRestriction?> GetByStateAsync(
            string state,
            CancellationToken cancellationToken = default)
        {
            return _context.StateLoanRestrictions
                .FirstOrDefaultAsync(
                    x => x.State == state &&
                         x.IsActive,
                    cancellationToken);
        }
    }
}
