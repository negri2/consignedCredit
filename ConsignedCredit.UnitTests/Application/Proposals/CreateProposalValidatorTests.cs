using ConsignedCredit.Application.Proposals.Create;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.UnitTests.Application.Proposals
{
    public class CreateProposalValidatorTests
    {
        private readonly CreateProposalValidator _validator = new();

        [Fact]
        public void Should_Reject_Invalid_Cpf()
        {
            var request = CreateValidRequest() with
            {
                Cpf = "123"
            };

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Cpf);
        }

        [Fact]
        public void Should_Reject_Invalid_Email()
        {
            var request = CreateValidRequest() with
            {
                Email = "invalid-email"
            };

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Reject_Empty_AgentId()
        {
            var request = CreateValidRequest() with
            {
                AgentId = Guid.Empty
            };

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.AgentId);
        }

        [Fact]
        public void Should_Reject_Invalid_State()
        {
            var request = CreateValidRequest() with
            {
                State = "Rio Grande do Sul"
            };

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.State);
        }

        [Fact]
        public void Should_Reject_More_Than_60_Installments()
        {
            var request = CreateValidRequest() with
            {
                Installments = 61
            };

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Installments);
        }

        [Fact]
        public void Should_Accept_Valid_Request()
        {
            var request = CreateValidRequest();

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
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
