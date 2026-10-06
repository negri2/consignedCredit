using ConsignedCredit.Domain.Entities;
using ConsignedCredit.Domain.Enums;
using ConsignedCredit.Domain.Exceptions;
using ConsignedCredit.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.UnitTests.Domain
{
    public class ProposalTests
    {
        [Fact]
        public void Should_Create_Proposal_With_Pending_Status()
        {
            var proponent = CreateValidProponent();

            var proposal = new Proposal(
                Guid.NewGuid(),
                Guid.NewGuid(),
                proponent);

            Assert.NotEqual(Guid.Empty, proposal.Id);
            Assert.Equal(ProposalStatus.Pending, proposal.Status);
            Assert.Equal(proponent, proposal.Proponent);
        }

        [Fact]
        public void Should_Throw_When_Agent_Is_Empty()
        {
            var act = () => new Proposal(
                Guid.Empty,
                Guid.NewGuid(),
                CreateValidProponent());

            var exception = Assert.Throws<DomainException>(act);

            Assert.Equal("Agent is required.", exception.Message);
        }

        [Fact]
        public void Should_Throw_When_Store_Is_Empty()
        {
            var act = () => new Proposal(
                Guid.NewGuid(),
                Guid.Empty,
                CreateValidProponent());

            var exception = Assert.Throws<DomainException>(act);

            Assert.Equal("Store is required.", exception.Message);
        }

        [Fact]
        public void Should_Throw_When_Proponent_Is_Null()
        {
            var act = () => new Proposal(
                Guid.NewGuid(),
                Guid.NewGuid(),
                null!);

            var exception = Assert.Throws<DomainException>(act);

            Assert.Equal("Proponent is required.", exception.Message);
        }

        private static Proponent CreateValidProponent()
        {
            var address = new Address(
                "Rua Teste",
                "123",
                "Caxias do Sul",
                "RS",
                "95000-000");

            return new Proponent(
                "12345678901",
                "123456789",
                3000m,
                new DateOnly(1960, 1, 1),
                "test@test.com",
                "54999999999",
                address);
        }
    }
}
