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
    public sealed class ProponentRepository : IProponentRepository
    {
        private readonly ConsignedCreditDbContext _context;

        public ProponentRepository(ConsignedCreditDbContext context)
        {
            _context = context;
        }

        public async Task<Proponent?> GetByCpfAsync(
            string cpf,
            CancellationToken cancellationToken = default)
        {
            return await _context.Proponents
                .FirstOrDefaultAsync(
                    x => x.Cpf == cpf,
                    cancellationToken);
        }

        public async Task AddAsync(
            Proponent proponent,
            CancellationToken cancellationToken = default)
        {
            await _context.Proponents.AddAsync(
                proponent,
                cancellationToken);
        }
    }
}
