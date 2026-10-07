using ConsignedCredit.Application.Abstractions.Persistence;
using ConsignedCredit.Application.Abstractions.Repositories;
using ConsignedCredit.Application.Abstractions.Services;
using ConsignedCredit.Application.Proposals.Process;
using ConsignedCredit.Domain.Entities;
using ConsignedCredit.Domain.Enums;
using ConsignedCredit.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.UnitTests.Application.Proposals
{
    public class ProcessProposalUseCaseTests
    {
        private readonly Mock<IProposalRepository> _proposalRepository;
        private readonly Mock<IUnitOfWork> _unitOfWork;

        private readonly Mock<ISimulationValidationService> _simulationValidationService;
        private readonly Mock<IRiskAnalysisService> _riskAnalysisService;
        private readonly Mock<IInssRegistrationService> _inssRegistrationService;
        private readonly Mock<IContractGenerationService> _contractGenerationService;
        private readonly Mock<IDigitalSignatureService> _digitalSignatureService;
        private readonly Mock<IPaymentService> _paymentService;
        private readonly Mock<ILogger<ProcessProposalUseCase>> _logger = new();

        private readonly ProcessProposalUseCase _useCase;

        public ProcessProposalUseCaseTests()
        {
            _proposalRepository = new Mock<IProposalRepository>();
            _unitOfWork = new Mock<IUnitOfWork>();

            _simulationValidationService = new Mock<ISimulationValidationService>();
            _riskAnalysisService = new Mock<IRiskAnalysisService>();
            _inssRegistrationService = new Mock<IInssRegistrationService>();
            _contractGenerationService = new Mock<IContractGenerationService>();
            _digitalSignatureService = new Mock<IDigitalSignatureService>();
            _paymentService = new Mock<IPaymentService>();

            _useCase = new ProcessProposalUseCase(
                _proposalRepository.Object,
                _unitOfWork.Object,
                _simulationValidationService.Object,
                _riskAnalysisService.Object,
                _inssRegistrationService.Object,
                _contractGenerationService.Object,
                _digitalSignatureService.Object,
                _paymentService.Object,
                _logger.Object);
        }

        [Fact]
        public async Task Should_Complete_Proposal_When_All_Steps_Succeed()
        {
            var proposal = CreateValidProposal();

            SetupProposal(proposal);
            SetupApprovedScores(proposal);

            await _useCase.ExecuteAsync(proposal.Id);

            Assert.Equal(ProposalStatus.Approved, proposal.Status);
            Assert.Equal(
                ProposalProcessingStep.Completed,
                proposal.ProcessingStep);

            _simulationValidationService.Verify(
                x => x.GetScoreAsync(
                    proposal.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _riskAnalysisService.Verify(
                x => x.GetScoreAsync(
                    proposal.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _inssRegistrationService.Verify(
                x => x.RegisterAsync(
                    proposal.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _contractGenerationService.Verify(
                x => x.GenerateAsync(
                    proposal.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _digitalSignatureService.Verify(
                x => x.SignAsync(
                    proposal.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _paymentService.Verify(
                x => x.PayAsync(
                    proposal.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Should_Reject_Proposal_When_Simulation_Score_Is_Below_Seven()
        {
            var proposal = CreateValidProposal();

            SetupProposal(proposal);

            _simulationValidationService
                .Setup(x => x.GetScoreAsync(
                    proposal.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(6);

            await _useCase.ExecuteAsync(proposal.Id);

            Assert.Equal(
                ProposalStatus.Rejected,
                proposal.Status);

            Assert.Equal(
                ProposalProcessingStep.SimulationValidation,
                proposal.ProcessingStep);

            Assert.Equal(
                "Simulation validation rejected with score 6.",
                proposal.RejectionReason);

            _riskAnalysisService.Verify(
                x => x.GetScoreAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            VerifyOperationalServicesWereNeverCalled();
        }

        [Fact]
        public async Task Should_Reject_Proposal_When_Risk_Score_Is_Below_Seven()
        {
            var proposal = CreateValidProposal();

            SetupProposal(proposal);

            _simulationValidationService
                .Setup(x => x.GetScoreAsync(
                    proposal.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(8);

            _riskAnalysisService
                .Setup(x => x.GetScoreAsync(
                    proposal.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(6);

            await _useCase.ExecuteAsync(proposal.Id);

            Assert.Equal(
                ProposalStatus.Rejected,
                proposal.Status);

            Assert.Equal(
                ProposalProcessingStep.RiskAnalysis,
                proposal.ProcessingStep);

            Assert.Equal(
                "Risk analysis rejected with score 6.",
                proposal.RejectionReason);

            VerifyOperationalServicesWereNeverCalled();
        }

        [Fact]
        public async Task Should_Resume_Proposal_From_Contract_Generation()
        {
            var proposal = CreateValidProposal();

            proposal.StartProcessing();
            proposal.CompleteSimulationValidation();
            proposal.CompleteRiskAnalysis();
            proposal.CompleteInssRegistration();

            SetupProposal(proposal);

            await _useCase.ExecuteAsync(proposal.Id);

            Assert.Equal(
                ProposalStatus.Approved,
                proposal.Status);

            Assert.Equal(
                ProposalProcessingStep.Completed,
                proposal.ProcessingStep);

            _simulationValidationService.Verify(
                x => x.GetScoreAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _riskAnalysisService.Verify(
                x => x.GetScoreAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _inssRegistrationService.Verify(
                x => x.RegisterAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _contractGenerationService.Verify(
                x => x.GenerateAsync(
                    proposal.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _digitalSignatureService.Verify(
                x => x.SignAsync(
                    proposal.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _paymentService.Verify(
                x => x.PayAsync(
                    proposal.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Should_Not_Process_Approved_Proposal()
        {
            var proposal = CreateCompletedProposal();

            SetupProposal(proposal);

            await _useCase.ExecuteAsync(proposal.Id);

            VerifyNoExternalServicesWereCalled();

            _unitOfWork.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Should_Not_Process_Rejected_Proposal()
        {
            var proposal = CreateValidProposal();

            proposal.StartProcessing();
            proposal.Reject("Rejected for test.");

            SetupProposal(proposal);

            await _useCase.ExecuteAsync(proposal.Id);

            VerifyNoExternalServicesWereCalled();

            _unitOfWork.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Should_Do_Nothing_When_Proposal_Does_Not_Exist()
        {
            var proposalId = Guid.NewGuid();

            _proposalRepository
                .Setup(x => x.GetByIdAsync(
                    proposalId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Proposal?)null);

            await _useCase.ExecuteAsync(proposalId);

            VerifyNoExternalServicesWereCalled();

            _unitOfWork.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private void SetupProposal(Proposal proposal)
        {
            _proposalRepository
                .Setup(x => x.GetByIdAsync(
                    proposal.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(proposal);
        }

        private void SetupApprovedScores(Proposal proposal)
        {
            _simulationValidationService
                .Setup(x => x.GetScoreAsync(
                    proposal.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(8);

            _riskAnalysisService
                .Setup(x => x.GetScoreAsync(
                    proposal.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(8);
        }

        private void VerifyOperationalServicesWereNeverCalled()
        {
            _inssRegistrationService.Verify(
                x => x.RegisterAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _contractGenerationService.Verify(
                x => x.GenerateAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _digitalSignatureService.Verify(
                x => x.SignAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _paymentService.Verify(
                x => x.PayAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private void VerifyNoExternalServicesWereCalled()
        {
            _simulationValidationService.Verify(
                x => x.GetScoreAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _riskAnalysisService.Verify(
                x => x.GetScoreAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            VerifyOperationalServicesWereNeverCalled();
        }

        private static Proposal CreateCompletedProposal()
        {
            var proposal = CreateValidProposal();

            proposal.StartProcessing();
            proposal.CompleteSimulationValidation();
            proposal.CompleteRiskAnalysis();
            proposal.CompleteInssRegistration();
            proposal.CompleteContractGeneration();
            proposal.CompleteDigitalSignature();
            proposal.CompletePayment();

            return proposal;
        }

        private static Proposal CreateValidProposal()
        {
            var address = new Address(
                "Rua Teste",
                "123",
                "Caxias do Sul",
                "RS",
                "95000-000");

            var proponent = new Proponent(
                "12345678901",
                "123456789",
                5000m,
                new DateOnly(1960, 1, 1),
                "test@test.com",
                "54999999999",
                address);

            var simulation = Simulation.Create(
                10_000m,
                48,
                proponent.RetirementIncome);

            return new Proposal(
                Guid.NewGuid(),
                Guid.NewGuid(),
                proponent,
                simulation,
                new DateTime(2026, 1, 1));
        }
    }
}
