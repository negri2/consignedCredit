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
    }
}
