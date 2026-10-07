using ConsignedCredit.Domain.Enums;
using ConsignedCredit.Domain.Exceptions;
using ConsignedCredit.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Domain.Entities
{
    public sealed class Proposal
    {
        public Guid Id { get; private set; }

        public Guid AgentId { get; private set; }
        public Guid StoreId { get; private set; }

        public Proponent Proponent { get; private set; }

        public Simulation Simulation { get; private set; }

        public ProposalStatus Status { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public ProposalProcessingStep ProcessingStep { get; private set; }
        public string? RejectionReason { get; private set; }

        private Proposal() { }

        public Proposal(
            Guid agentId,
            Guid storeId,
            Proponent proponent,
            Simulation simulation,
            DateTime? createdAt = null)
        {
            if (agentId == Guid.Empty)
                throw new DomainException("Agent is required.");

            if (storeId == Guid.Empty)
                throw new DomainException("Store is required.");

            Proponent = proponent
                ?? throw new DomainException("Proponent is required.");

            Simulation = simulation
                ?? throw new DomainException("Simulation is required.");

            Id = Guid.NewGuid();
            AgentId = agentId;
            StoreId = storeId;

            CreatedAt = createdAt ?? DateTime.UtcNow;

            ValidateMaximumAge();

            Status = ProposalStatus.Pending;
            ProcessingStep = ProposalProcessingStep.None;
        }

        private void ValidateMaximumAge()
        {
            var lastInstallmentDate =
                CreatedAt.AddMonths(Simulation.Installments);

            var eightyBirthday =
                Proponent.BirthDate.AddYears(80);

            if (DateOnly.FromDateTime(lastInstallmentDate) > eightyBirthday)
                throw new DomainException(
                    "Last installment cannot exceed proponent age of 80.");
        }

        public void StartProcessing()
        {
            if (Status != ProposalStatus.Pending)
                throw new DomainException(
                    "Only pending proposals can start processing.");

            Status = ProposalStatus.Processing;
            ProcessingStep = ProposalProcessingStep.SimulationValidation;
        }

        public void CompleteSimulationValidation()
        {
            EnsureCurrentStep(ProposalProcessingStep.SimulationValidation);

            ProcessingStep = ProposalProcessingStep.RiskAnalysis;
        }

        public void CompleteRiskAnalysis()
        {
            EnsureCurrentStep(ProposalProcessingStep.RiskAnalysis);

            ProcessingStep = ProposalProcessingStep.InssRegistration;
        }

        public void CompleteInssRegistration()
        {
            EnsureCurrentStep(ProposalProcessingStep.InssRegistration);

            ProcessingStep = ProposalProcessingStep.ContractGeneration;
        }

        public void CompleteContractGeneration()
        {
            EnsureCurrentStep(ProposalProcessingStep.ContractGeneration);

            ProcessingStep = ProposalProcessingStep.DigitalSignature;
        }

        public void CompleteDigitalSignature()
        {
            EnsureCurrentStep(ProposalProcessingStep.DigitalSignature);

            ProcessingStep = ProposalProcessingStep.Payment;
        }

        public void CompletePayment()
        {
            EnsureCurrentStep(ProposalProcessingStep.Payment);

            ProcessingStep = ProposalProcessingStep.Completed;
            Status = ProposalStatus.Approved;
        }

        public void Reject(string reason)
        {
            if (Status != ProposalStatus.Processing)
                throw new DomainException(
                    "Only processing proposals can be rejected.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException(
                    "Rejection reason is required.");

            Status = ProposalStatus.Rejected;
            RejectionReason = reason;
        }

        private void EnsureCurrentStep(ProposalProcessingStep expectedStep)
        {
            if (Status != ProposalStatus.Processing ||
                ProcessingStep != expectedStep)
            {
                throw new DomainException(
                    $"Proposal is not in the expected processing step: {expectedStep}.");
            }
        }
    }
}
