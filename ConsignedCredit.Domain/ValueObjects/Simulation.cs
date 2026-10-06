using ConsignedCredit.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Domain.ValueObjects
{
    public sealed class Simulation
    {
        public const int MaximumInstallments = 60;
        public const decimal MaximumIncomeCommitment = 0.30m;
        public const decimal AnnualInterestRate = 0.12m;

        public decimal RequestedAmount { get; private set; }
        public int Installments { get; private set; }
        public decimal InstallmentAmount { get; private set; }

        private Simulation() { }

        public static Simulation Create(
            decimal requestedAmount,
            int installments,
            decimal retirementIncome)
        {
            if (requestedAmount <= 0)
                throw new DomainException(
                    "Requested amount must be greater than zero.");

            if (installments <= 0 || installments > MaximumInstallments)
                throw new DomainException(
                    $"Installments must be between 1 and {MaximumInstallments}.");

            if (retirementIncome <= 0)
                throw new DomainException(
                    "Retirement income must be greater than zero.");

            var installmentAmount =
                CalculateInstallment(requestedAmount, installments);

            var maximumInstallment =
                retirementIncome * MaximumIncomeCommitment;

            if (installmentAmount > maximumInstallment)
                throw new DomainException(
                    "Installment amount exceeds 30% of retirement income.");

            return new Simulation
            {
                RequestedAmount = requestedAmount,
                Installments = installments,
                InstallmentAmount = installmentAmount
            };
        }

        private static decimal CalculateInstallment(
            decimal requestedAmount,
            int installments)
        {
            // Price system
            var monthlyRate =
                Math.Pow(1 + (double)AnnualInterestRate, 1.0 / 12.0) - 1;

            var factor = Math.Pow(1 + monthlyRate, installments);

            var installment =
                (double)requestedAmount *
                (monthlyRate * factor) /
                (factor - 1);

            return Math.Round((decimal)installment, 2);
        }
    }
}
