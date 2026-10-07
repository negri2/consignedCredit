using ConsignedCredit.Application.Abstractions.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Application.Proposals.Get
{
    public sealed class GetProposalUseCase
    {
        private readonly IProposalRepository _proposalRepository;

        public GetProposalUseCase(
            IProposalRepository proposalRepository)
        {
            _proposalRepository = proposalRepository;
        }

        public async Task<GetProposalResult?> ExecuteAsync(
            Guid proposalId,
            CancellationToken cancellationToken = default)
        {
            var proposal = await _proposalRepository.GetByIdAsync(
                proposalId,
                cancellationToken);

            if (proposal is null)
                return null;

            return new GetProposalResult(
                proposal.Id,
                proposal.Proponent.Cpf,
                proposal.Simulation.RequestedAmount,
                proposal.Simulation.Installments,
                proposal.Simulation.InstallmentAmount,
                proposal.Status.ToString(),
                proposal.ProcessingStep.ToString(),
                proposal.RejectionReason,
                proposal.CreatedAt);
        }
    }
}
