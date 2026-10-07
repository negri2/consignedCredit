using ConsignedCredit.Application.Abstractions.Persistence;
using ConsignedCredit.Application.Abstractions.Repositories;
using ConsignedCredit.Application.Abstractions.Services;
using ConsignedCredit.Domain.Enums;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ProcessProposalUseCase> _logger;

        public ProcessProposalUseCase(
            IProposalRepository proposalRepository,
            IUnitOfWork unitOfWork,
            ISimulationValidationService simulationValidationService,
            IRiskAnalysisService riskAnalysisService,
            IInssRegistrationService inssRegistrationService,
            IContractGenerationService contractGenerationService,
            IDigitalSignatureService digitalSignatureService,
            IPaymentService paymentService,
            ILogger<ProcessProposalUseCase> logger)
        {
            _proposalRepository = proposalRepository;
            _unitOfWork = unitOfWork;
            _simulationValidationService = simulationValidationService;
            _riskAnalysisService = riskAnalysisService;
            _inssRegistrationService = inssRegistrationService;
            _contractGenerationService = contractGenerationService;
            _digitalSignatureService = digitalSignatureService;
            _paymentService = paymentService;
            _logger = logger;
        }

        public async Task ExecuteAsync(
            Guid proposalId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Starting processing for proposal {ProposalId}.",
                proposalId);

            var proposal = await _proposalRepository.GetByIdAsync(
                proposalId,
                cancellationToken);

            if (proposal is null)
            {
                _logger.LogWarning(
                    "Proposal {ProposalId} was not found.",
                    proposalId);

                return;
            }

            if (proposal.Status is ProposalStatus.Approved or ProposalStatus.Rejected)
            {
                _logger.LogInformation(
                    "Proposal {ProposalId} is already finalized with status {Status}. Skipping processing.",
                    proposal.Id,
                    proposal.Status);

                return;
            }

            if (proposal.Status == ProposalStatus.Pending)
            {
                proposal.StartProcessing();

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Proposal {ProposalId} processing started.",
                    proposal.Id);
            }

            if (proposal.ProcessingStep == ProposalProcessingStep.SimulationValidation)
            {
                var score = await _simulationValidationService.GetScoreAsync(
                    proposal.Id,
                    cancellationToken);

                _logger.LogInformation(
                    "Proposal {ProposalId} simulation validation completed with score {Score}.",
                    proposal.Id,
                    score);

                if (score < MinimumApprovalScore)
                {
                    proposal.Reject(
                        $"Simulation validation rejected with score {score}.");

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogWarning(
                        "Proposal {ProposalId} rejected during simulation validation with score {Score}.",
                        proposal.Id,
                        score);

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

                _logger.LogInformation(
                    "Proposal {ProposalId} risk analysis completed with score {Score}.",
                    proposal.Id,
                    score);

                if (score < MinimumApprovalScore)
                {
                    proposal.Reject(
                        $"Risk analysis rejected with score {score}.");

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogWarning(
                        "Proposal {ProposalId} rejected during risk analysis with score {Score}.",
                        proposal.Id,
                        score);

                    return;
                }

                proposal.CompleteRiskAnalysis();

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            if (proposal.ProcessingStep == ProposalProcessingStep.InssRegistration)
            {
                _logger.LogInformation(
                    "Registering proposal {ProposalId} with INSS.",
                    proposal.Id);

                await _inssRegistrationService.RegisterAsync(
                    proposal.Id,
                    cancellationToken);

                proposal.CompleteInssRegistration();

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Proposal {ProposalId} successfully registered with INSS.",
                    proposal.Id);
            }

            if (proposal.ProcessingStep == ProposalProcessingStep.ContractGeneration)
            {
                _logger.LogInformation(
                    "Generating contract for proposal {ProposalId}.",
                    proposal.Id);

                await _contractGenerationService.GenerateAsync(
                    proposal.Id,
                    cancellationToken);

                proposal.CompleteContractGeneration();

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Contract generated for proposal {ProposalId}.",
                    proposal.Id);
            }

            if (proposal.ProcessingStep == ProposalProcessingStep.DigitalSignature)
            {
                _logger.LogInformation(
                    "Processing digital signature for proposal {ProposalId}.",
                    proposal.Id);

                await _digitalSignatureService.SignAsync(
                    proposal.Id,
                    cancellationToken);

                proposal.CompleteDigitalSignature();

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Digital signature completed for proposal {ProposalId}.",
                    proposal.Id);
            }

            if (proposal.ProcessingStep == ProposalProcessingStep.Payment)
            {
                _logger.LogInformation(
                    "Processing payment for proposal {ProposalId}.",
                    proposal.Id);

                await _paymentService.PayAsync(
                    proposal.Id,
                    cancellationToken);

                proposal.CompletePayment();

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Proposal {ProposalId} processing completed successfully with status {Status}.",
                    proposal.Id,
                    proposal.Status);
            }
        }
    }
}
