using ConsignedCredit.Application.Abstractions.Persistence;
using ConsignedCredit.Application.Abstractions.Repositories;
using ConsignedCredit.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Application.Proposals.Process
{
    public sealed class ProcessProposalUseCase
    {
        private readonly IProposalRepository _proposalRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProcessProposalUseCase(
            IProposalRepository proposalRepository,
            IUnitOfWork unitOfWork)
        {
            _proposalRepository = proposalRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync(
            Guid proposalId,
            CancellationToken cancellationToken = default)
        {
            var proposal = await _proposalRepository.GetByIdAsync(
                proposalId,
                cancellationToken);

            if (proposal is null)
                return;

            if (proposal.Status != ProposalStatus.Pending)
                return;

            proposal.StartProcessing();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
