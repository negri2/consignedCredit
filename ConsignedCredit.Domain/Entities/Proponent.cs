using ConsignedCredit.Domain.Exceptions;
using ConsignedCredit.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Domain.Entities
{
    public sealed class Proponent
    {
        public Guid Id { get; private set; }
        public string Cpf { get; private set; }
        public string InssNumber { get; private set; }
        public decimal RetirementIncome { get; private set; }
        public DateOnly BirthDate { get; private set; }
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public Address Address { get; private set; }

        private Proponent() { }

        public Proponent(
            string cpf,
            string inssNumber,
            decimal retirementIncome,
            DateOnly birthDate,
            string email,
            string phone,
            Address address)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                throw new DomainException("CPF is required.");

            if (string.IsNullOrWhiteSpace(inssNumber))
                throw new DomainException("INSS number is required.");

            if (retirementIncome <= 0)
                throw new DomainException(
                    "Retirement income must be greater than zero.");

            if (birthDate == default)
                throw new DomainException("Birth date is required.");

            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("Email is required.");

            if (string.IsNullOrWhiteSpace(phone))
                throw new DomainException("Phone is required.");

            Address = address
                ?? throw new DomainException("Address is required.");

            Cpf = cpf;
            InssNumber = inssNumber;
            RetirementIncome = retirementIncome;
            BirthDate = birthDate;
            Email = email;
            Phone = phone;
        }

        public void Update(
            string inssNumber,
            decimal retirementIncome,
            string email,
            string phone,
            Address address)
        {
            if (string.IsNullOrWhiteSpace(inssNumber))
                throw new DomainException("INSS number is required.");

            if (retirementIncome <= 0)
                throw new DomainException(
                    "Retirement income must be greater than zero.");

            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("Email is required.");

            if (string.IsNullOrWhiteSpace(phone))
                throw new DomainException("Phone is required.");

            InssNumber = inssNumber;
            RetirementIncome = retirementIncome;
            Email = email;
            Phone = phone;
            Address = address
                ?? throw new DomainException("Address is required.");
        }
    }
}
