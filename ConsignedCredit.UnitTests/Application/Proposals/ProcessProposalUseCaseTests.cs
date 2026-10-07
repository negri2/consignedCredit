using ConsignedCredit.Application.Abstractions.Persistence;
using ConsignedCredit.Application.Abstractions.Repositories;
using ConsignedCredit.Application.Proposals.Process;
using ConsignedCredit.Domain.Entities;
using ConsignedCredit.Domain.Enums;
using ConsignedCredit.Domain.ValueObjects;
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
        private readonly ProcessProposalUseCase _useCase;

        public ProcessProposalUseCaseTests()
        {
            _proposalRepository = new Mock<IProposalRepository>();
            _unitOfWork = new Mock<IUnitOfWork>();

            _useCase = new ProcessProposalUseCase(
                _proposalRepository.Object,
                _unitOfWork.Object);
        }

        [Fact]
        public async Task Should_Start_Processing_Pending_Proposal()
        {
            var proposal = CreateValidProposal();

            _proposalRepository
                .Setup(x => x.GetByIdAsync(
                    proposal.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(proposal);

            await _useCase.ExecuteAsync(proposal.Id);

            Assert.Equal(
                ProposalStatus.Processing,
                proposal.Status);

            Assert.Equal(
                ProposalProcessingStep.SimulationValidation,
                proposal.ProcessingStep);

            _unitOfWork.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Should_Not_Process_Proposal_That_Is_Already_Processing()
        {
            var proposal = CreateValidProposal();

            proposal.StartProcessing();

            _proposalRepository
                .Setup(x => x.GetByIdAsync(
                    proposal.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(proposal);

            await _useCase.ExecuteAsync(proposal.Id);

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

            _unitOfWork.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
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
