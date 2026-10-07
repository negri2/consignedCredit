using ConsignedCredit.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Domain.Entities
{
    public sealed class StateLoanRestriction
    {
        public Guid Id { get; private set; }
        public string State { get; private set; }
        public decimal MaximumAmount { get; private set; }
        public bool IsActive { get; private set; }

        private StateLoanRestriction() { }

        public StateLoanRestriction(
            string state,
            decimal maximumAmount)
        {
            if (string.IsNullOrWhiteSpace(state))
                throw new DomainException("State is required.");

            if (state.Length != 2)
                throw new DomainException("State must contain 2 characters.");

            if (maximumAmount <= 0)
                throw new DomainException(
                    "Maximum amount must be greater than zero.");

            Id = Guid.NewGuid();
            State = state.ToUpperInvariant();
            MaximumAmount = maximumAmount;
            IsActive = true;
        }
    }
}
