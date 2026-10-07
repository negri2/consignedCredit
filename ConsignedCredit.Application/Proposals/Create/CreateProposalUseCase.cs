using ConsignedCredit.Application.Abstractions.Persistence;
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
        private readonly IProponentRepository _proponentRepository;
        private readonly IAgentService _agentService;
        private readonly IFraudCheckService _fraudCheckService;
        private readonly IUnitOfWork _unitOfWork;

        public CreateProposalUseCase(
            IProposalRepository proposalRepository,
            IProponentRepository proponentRepository,
            IAgentService agentService,
            IFraudCheckService fraudCheckService,
            IUnitOfWork unitOfWork)
        {
            _proposalRepository = proposalRepository;
            _proponentRepository = proponentRepository;
            _agentService = agentService;
            _fraudCheckService = fraudCheckService;
            _unitOfWork = unitOfWork;
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

            var proponent = await _proponentRepository.GetByCpfAsync(
                request.Cpf,
                cancellationToken);

            if (proponent is null)
            {
                proponent = new Proponent(
                    request.Cpf,
                    request.InssNumber,
                    request.RetirementIncome,
                    request.BirthDate,
                    request.Email,
                    request.Phone,
                    address);

                await _proponentRepository.AddAsync(
                    proponent,
                    cancellationToken);
            }
            else
            {
                proponent.Update(
                    request.InssNumber,
                    request.RetirementIncome,
                    request.Email,
                    request.Phone,
                    address);
            }

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

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return proposal.Id;
        }
    }
}