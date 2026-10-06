using ConsignedCredit.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Application.Abstractions.Repositories
{
    public interface IProposalRepository
    {
        Task<bool> HasOpenProposalByCpfAsync(
            string cpf,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            Proposal proposal,
            CancellationToken cancellationToken = default);
    }
}
