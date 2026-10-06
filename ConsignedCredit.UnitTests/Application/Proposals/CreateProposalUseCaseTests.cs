using ConsignedCredit.Application.Abstractions.Repositories;
using ConsignedCredit.Application.Abstractions.Services;
using ConsignedCredit.Application.Exceptions;
using ConsignedCredit.Application.Proposals.Create;
using ConsignedCredit.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace ConsignedCredit.UnitTests.Application.Proposals
{
    public class CreateProposalUseCaseTests
    {
        private readonly Mock<IProposalRepository> _proposalRepository;
        private readonly Mock<IAgentService> _agentService;
        private readonly Mock<IFraudCheckService> _fraudCheckService;

        private readonly CreateProposalUseCase _useCase;

        public CreateProposalUseCaseTests()
        {
            _proposalRepository = new Mock<IProposalRepository>();
            _agentService = new Mock<IAgentService>();
            _fraudCheckService = new Mock<IFraudCheckService>();

            _useCase = new CreateProposalUseCase(
                _proposalRepository.Object,
                _agentService.Object,
                _fraudCheckService.Object);
        }

        [Fact]
        public async Task Should_Create_Proposal_When_Data_Is_Valid()
        {
            var request = CreateValidRequest();
            Proposal? savedProposal = null;

            _agentService
                .Setup(x => x.IsActiveAsync(
                    request.AgentId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _proposalRepository
                .Setup(x => x.HasOpenProposalByCpfAsync(
                    request.Cpf,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _fraudCheckService
                .Setup(x => x.IsFraudulentAsync(
                    request.Cpf,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _proposalRepository
                .Setup(x => x.AddAsync(
                    It.IsAny<Proposal>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Proposal, CancellationToken>(
                    (proposal, _) => savedProposal = proposal);

            var proposalId = await _useCase.ExecuteAsync(request);

            Assert.NotEqual(Guid.Empty, proposalId);

            Assert.NotNull(savedProposal);
            Assert.Equal(proposalId, savedProposal.Id);
            Assert.Equal(request.AgentId, savedProposal.AgentId);
            Assert.Equal(request.StoreId, savedProposal.StoreId);
            Assert.Equal(request.Cpf, savedProposal.Proponent.Cpf);
            Assert.Equal(request.RequestedAmount, savedProposal.Simulation.RequestedAmount);
            Assert.Equal(request.Installments, savedProposal.Simulation.Installments);

            _proposalRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<Proposal>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Should_Throw_When_Agent_Is_Not_Active()
        {
            var request = CreateValidRequest();

            _agentService
                .Setup(x => x.IsActiveAsync(
                    request.AgentId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(
                () => _useCase.ExecuteAsync(request));

            Assert.Equal("Agent is not active.", exception.Message);

            _proposalRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<Proposal>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public async Task Should_Throw_When_Proponent_Has_Open_Proposal()
        {
            var request = CreateValidRequest();

            _agentService
                .Setup(x => x.IsActiveAsync(
                    request.AgentId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _proposalRepository
                .Setup(x => x.HasOpenProposalByCpfAsync(
                    request.Cpf,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(
                () => _useCase.ExecuteAsync(request));

            Assert.Equal(
                "Proponent already has an open proposal.",
                exception.Message);

            _proposalRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<Proposal>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Should_Throw_When_Cpf_Is_Fraudulent()
        {
            var request = CreateValidRequest();

            _agentService
                .Setup(x => x.IsActiveAsync(
                    request.AgentId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _proposalRepository
                .Setup(x => x.HasOpenProposalByCpfAsync(
                    request.Cpf,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _fraudCheckService
                .Setup(x => x.IsFraudulentAsync(
                    request.Cpf,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(
                () => _useCase.ExecuteAsync(request));

            Assert.Equal(
                "Proponent CPF is blocked by fraud check.",
                exception.Message);

            _proposalRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<Proposal>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private static CreateProposalRequest CreateValidRequest()
        {
            return new CreateProposalRequest(
                AgentId: Guid.NewGuid(),
                StoreId: Guid.NewGuid(),
                Cpf: "12345678901",
                InssNumber: "123456789",
                RetirementIncome: 5000m,
                BirthDate: new DateOnly(1960, 1, 1),
                Email: "test@test.com",
                Phone: "54999999999",
                Street: "Rua Teste",
                Number: "123",
                City: "Caxias do Sul",
                State: "RS",
                ZipCode: "95000-000",
                RequestedAmount: 10_000m,
                Installments: 48);
        }
    }
}
