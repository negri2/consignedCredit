using ConsignedCredit.Application.Abstractions.Repositories;
using ConsignedCredit.Domain.Entities;
using ConsignedCredit.Domain.Enums;
using ConsignedCredit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Infrastructure.Repositories
{
    public sealed class ProposalRepository : IProposalRepository
    {
        private readonly ConsignedCreditDbContext _context;

        public ProposalRepository(ConsignedCreditDbContext context)
        {
            _context = context;
        }

        public async Task<Proposal?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Proposals
                .Include(x => x.Proponent)
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        public async Task<bool> HasOpenProposalByCpfAsync(
            string cpf,
            CancellationToken cancellationToken = default)
        {
            return await _context.Proposals
                .AnyAsync(
                    x => x.Proponent.Cpf == cpf &&
                         (x.Status == ProposalStatus.Pending ||
                          x.Status == ProposalStatus.Processing),
                    cancellationToken);
        }

        public async Task AddAsync(
            Proposal proposal,
            CancellationToken cancellationToken = default)
        {
            await _context.Proposals.AddAsync(
                proposal,
                cancellationToken);
        }
    }
}
