using ConsignedCredit.Application.Abstractions.Messaging;
using ConsignedCredit.Application.Abstractions.Persistence;
using ConsignedCredit.Application.Abstractions.Repositories;
using ConsignedCredit.Application.Abstractions.Services;
using ConsignedCredit.Application.Exceptions;
using ConsignedCredit.Application.Proposals.Create;
using ConsignedCredit.Application.Proposals.Events;
using ConsignedCredit.Domain.Entities;
using ConsignedCredit.Domain.ValueObjects;
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
        private readonly Mock<IProponentRepository> _proponentRepository;
        private readonly Mock<IAgentService> _agentService;
        private readonly Mock<IFraudCheckService> _fraudCheckService;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IOutbox> _outbox;

        private readonly CreateProposalUseCase _useCase;

        public CreateProposalUseCaseTests()
        {
            _proposalRepository = new Mock<IProposalRepository>();
            _proponentRepository = new Mock<IProponentRepository>();
            _agentService = new Mock<IAgentService>();
            _fraudCheckService = new Mock<IFraudCheckService>();
            _unitOfWork = new Mock<IUnitOfWork>();
            _outbox = new Mock<IOutbox>();

            _useCase = new CreateProposalUseCase(
                _proposalRepository.Object,
                _proponentRepository.Object,
                _agentService.Object,
                _fraudCheckService.Object,
                _unitOfWork.Object,
                _outbox.Object);
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

            _proponentRepository
                .Setup(x => x.GetByCpfAsync(
                    request.Cpf,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Proponent?)null);

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

            _proponentRepository.Verify(
                x => x.AddAsync(
                    It.Is<Proponent>(p => p.Cpf == request.Cpf),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWork.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _outbox.Verify(
                x => x.AddAsync(
                    It.Is<ProposalCreatedEvent>(
                        e => e.ProposalId == proposalId),
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

            _unitOfWork.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _outbox.Verify(
                x => x.AddAsync(
                    It.IsAny<ProposalCreatedEvent>(),
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

            _unitOfWork.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _outbox.Verify(
                x => x.AddAsync(
                    It.IsAny<ProposalCreatedEvent>(),
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

            _unitOfWork.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _outbox.Verify(
                x => x.AddAsync(
                    It.IsAny<ProposalCreatedEvent>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Should_Reuse_Existing_Proponent()
        {
            var request = CreateValidRequest();

            var address = new Address(
                "Rua Antiga",
                "10",
                "Caxias do Sul",
                "RS",
                "95000-000");

            var existingProponent = new Proponent(
                request.Cpf,
                request.InssNumber,
                4000m,
                request.BirthDate,
                "old@test.com",
                "54988888888",
                address);

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

            _proponentRepository
                .Setup(x => x.GetByCpfAsync(
                    request.Cpf,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProponent);

            var proposalId = await _useCase.ExecuteAsync(request);

            // Não cria outro proponente
            _proponentRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<Proponent>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            // Atualiza os dados do proponente existente
            Assert.Equal(
                request.RetirementIncome,
                existingProponent.RetirementIncome);

            Assert.Equal(
                request.Email,
                existingProponent.Email);

            // Mas cria uma nova proposta
            _proposalRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<Proposal>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Persiste tudo
            _unitOfWork.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _outbox.Verify(
                x => x.AddAsync(
                    It.Is<ProposalCreatedEvent>(
                        e => e.ProposalId == proposalId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
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
