using ConsignedCredit.Application.Abstractions.Persistence;
using ConsignedCredit.Application.Abstractions.Repositories;
using ConsignedCredit.Application.Abstractions.Services;
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
        private const int MinimumApprovalScore = 7;

        private readonly IProposalRepository _proposalRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISimulationValidationService _simulationValidationService;
        private readonly IRiskAnalysisService _riskAnalysisService;
        private readonly IInssRegistrationService _inssRegistrationService;
        private readonly IContractGenerationService _contractGenerationService;
        private readonly IDigitalSignatureService _digitalSignatureService;
        private readonly IPaymentService _paymentService;

        public ProcessProposalUseCase(
            IProposalRepository proposalRepository,
            IUnitOfWork unitOfWork,
            ISimulationValidationService simulationValidationService,
            IRiskAnalysisService riskAnalysisService,
            IInssRegistrationService inssRegistrationService,
            IContractGenerationService contractGenerationService,
            IDigitalSignatureService digitalSignatureService,
            IPaymentService paymentService)
        {
            _proposalRepository = proposalRepository;
            _unitOfWork = unitOfWork;
            _simulationValidationService = simulationValidationService;
            _riskAnalysisService = riskAnalysisService;
            _inssRegistrationService = inssRegistrationService;
            _contractGenerationService = contractGenerationService;
            _digitalSignatureService = digitalSignatureService;
            _paymentService = paymentService;
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

            if (proposal.Status is ProposalStatus.Approved or ProposalStatus.Rejected)
                return;

            if (proposal.Status == ProposalStatus.Pending)
            {
                proposal.StartProcessing();

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            if (proposal.ProcessingStep == ProposalProcessingStep.SimulationValidation)
            {
                var score = await _simulationValidationService.GetScoreAsync(
                    proposal.Id,
                    cancellationToken);

                if (score < MinimumApprovalScore)
                {
                    proposal.Reject(
                        $"Simulation validation rejected with score {score}.");

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    return;
                }

                proposal.CompleteSimulationValidation();

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            if (proposal.ProcessingStep == ProposalProcessingStep.RiskAnalysis)
            {
                var score = await _riskAnalysisService.GetScoreAsync(
                    proposal.Id,
                    cancellationToken);

                if (score < MinimumApprovalScore)
                {
                    proposal.Reject(
                        $"Risk analysis rejected with score {score}.");

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    return;
                }

                proposal.CompleteRiskAnalysis();

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            if (proposal.ProcessingStep == ProposalProcessingStep.InssRegistration)
            {
                await _inssRegistrationService.RegisterAsync(
                    proposal.Id,
                    cancellationToken);

                proposal.CompleteInssRegistration();

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            if (proposal.ProcessingStep == ProposalProcessingStep.ContractGeneration)
            {
                await _contractGenerationService.GenerateAsync(
                    proposal.Id,
                    cancellationToken);

                proposal.CompleteContractGeneration();

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            if (proposal.ProcessingStep == ProposalProcessingStep.DigitalSignature)
            {
                await _digitalSignatureService.SignAsync(
                    proposal.Id,
                    cancellationToken);

                proposal.CompleteDigitalSignature();

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            if (proposal.ProcessingStep == ProposalProcessingStep.Payment)
            {
                await _paymentService.PayAsync(
                    proposal.Id,
                    cancellationToken);

                proposal.CompletePayment();

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
