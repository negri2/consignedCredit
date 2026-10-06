using ConsignedCredit.Application.Abstractions.Repositories;
using ConsignedCredit.Application.Abstractions.Services;
using ConsignedCredit.Application.Exceptions;
using ConsignedCredit.Domain.Entities;
using ConsignedCredit.Domain.Exceptions;
using ConsignedCredit.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Application.Proposals.Create
{
    public sealed class CreateProposalUseCase
    {
        private readonly IProposalRepository _proposalRepository;
        private readonly IAgentService _agentService;
        private readonly IFraudCheckService _fraudCheckService;

        public CreateProposalUseCase(
            IProposalRepository proposalRepository,
            IAgentService agentService,
            IFraudCheckService fraudCheckService)
        {
            _proposalRepository = proposalRepository;
            _agentService = agentService;
            _fraudCheckService = fraudCheckService;
        }

        public async Task<Guid> ExecuteAsync(
            CreateProposalRequest request,
            CancellationToken cancellationToken = default)
        {
            var agentIsActive = await _agentService.IsActiveAsync(
                request.AgentId,
                cancellationToken);

            if (!agentIsActive)
                throw new BusinessRuleException("Agent is not active.");

            var hasOpenProposal =
                await _proposalRepository.HasOpenProposalByCpfAsync(
                    request.Cpf,
                    cancellationToken);

            if (hasOpenProposal)
                throw new BusinessRuleException(
                    "Proponent already has an open proposal.");

            var isFraudulent =
                await _fraudCheckService.IsFraudulentAsync(
                    request.Cpf,
                    cancellationToken);

            if (isFraudulent)
                throw new BusinessRuleException(
                    "Proponent CPF is blocked by fraud check.");

            var address = new Address(
                request.Street,
                request.Number,
                request.City,
                request.State,
                request.ZipCode);

            var proponent = new Proponent(
                request.Cpf,
                request.InssNumber,
                request.RetirementIncome,
                request.BirthDate,
                request.Email,
                request.Phone,
                address);

            var simulation = Simulation.Create(
                request.RequestedAmount,
                request.Installments,
                request.RetirementIncome);

            var proposal = new Proposal(
                request.AgentId,
                request.StoreId,
                proponent,
                simulation);

            await _proposalRepository.AddAsync(
                proposal,
                cancellationToken);

            return proposal.Id;
        }
    }
}