using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Application.Proposals.Create
{
    public sealed class CreateProposalValidator
        : AbstractValidator<CreateProposalRequest>
    {
        public CreateProposalValidator()
        {
            RuleFor(x => x.AgentId)
                .NotEmpty();

            RuleFor(x => x.StoreId)
                .NotEmpty();

            RuleFor(x => x.Cpf)
                .NotEmpty()
                .Matches(@"^\d{11}$")
                .WithMessage("CPF must contain exactly 11 digits.");

            RuleFor(x => x.InssNumber)
                .NotEmpty();

            RuleFor(x => x.RetirementIncome)
                .GreaterThan(0);

            RuleFor(x => x.BirthDate)
                .NotEmpty()
                .LessThan(DateOnly.FromDateTime(DateTime.Today));

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Phone)
                .NotEmpty();

            RuleFor(x => x.Street)
                .NotEmpty();

            RuleFor(x => x.Number)
                .NotEmpty();

            RuleFor(x => x.City)
                .NotEmpty();

            RuleFor(x => x.State)
                .NotEmpty()
                .Length(2);

            RuleFor(x => x.ZipCode)
                .NotEmpty();

            RuleFor(x => x.RequestedAmount)
                .GreaterThan(0);

            RuleFor(x => x.Installments)
                .InclusiveBetween(1, 60);
        }
    }
}
