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
            var simulation = CreateValidSimulation(proponent);

            var proposal = new Proposal(
                Guid.NewGuid(),
                Guid.NewGuid(),
                proponent,
                simulation);

            Assert.NotEqual(Guid.Empty, proposal.Id);
            Assert.Equal(ProposalStatus.Pending, proposal.Status);
            Assert.Equal(proponent, proposal.Proponent);
        }

        [Fact]
        public void Should_Throw_When_Agent_Is_Empty()
        {
            var proponent = CreateValidProponent();
            var simulation = CreateValidSimulation(proponent);

            var act = () => new Proposal(
                Guid.Empty,
                Guid.NewGuid(),
                proponent,
                simulation);

            var exception = Assert.Throws<DomainException>(act);

            Assert.Equal("Agent is required.", exception.Message);
        }

        [Fact]
        public void Should_Throw_When_Store_Is_Empty()
        {
            var proponent = CreateValidProponent();
            var simulation = CreateValidSimulation(proponent);

            var act = () => new Proposal(
                Guid.NewGuid(),
                Guid.Empty,
                proponent, 
                simulation);

            var exception = Assert.Throws<DomainException>(act);

            Assert.Equal("Store is required.", exception.Message);
        }

        [Fact]
        public void Should_Throw_When_Proponent_Is_Null()
        {
            var simulation = Simulation.Create(
                10_000m,
                48,
                3000m);

            var act = () => new Proposal(
                Guid.NewGuid(),
                Guid.NewGuid(),
                null!,
                simulation);

            var exception = Assert.Throws<DomainException>(act);

            Assert.Equal("Proponent is required.", exception.Message);
        }

        [Fact]
        public void Should_Throw_When_Simulation_Is_Null()
        {
            var proponent = CreateValidProponent();

            var act = () => new Proposal(
                Guid.NewGuid(),
                Guid.NewGuid(),
                proponent,
                null!);

            var exception = Assert.Throws<DomainException>(act);

            Assert.Equal("Simulation is required.", exception.Message);
        }

        [Fact]
        public void Should_Throw_When_Last_Installment_Exceeds_Age_80()
        {
            var address = new Address(
                "Rua Teste",
                "123",
                "Caxias do Sul",
                "RS",
                "95000-000");

            var proponent = new Proponent(
                "12345678901",
                "123456789",
                5000m,
                new DateOnly(1950, 1, 1),
                "test@test.com",
                "54999999999",
                address);

            var simulation = Simulation.Create(
                10_000m,
                60,
                proponent.RetirementIncome);

            var act = () => new Proposal(
                Guid.NewGuid(),
                Guid.NewGuid(),
                proponent,
                simulation,
                new DateTime(2026, 1, 1));

            var exception = Assert.Throws<DomainException>(act);

            Assert.Equal(
                "Last installment cannot exceed proponent age of 80.",
                exception.Message);
        }

        [Fact]
        public void Should_Create_When_Last_Installment_Is_Before_Age_80()
        {
            var proponent = CreateValidProponent();
            var simulation = CreateValidSimulation(proponent);

            var proposal = new Proposal(
                Guid.NewGuid(),
                Guid.NewGuid(),
                proponent,
                simulation,
                new DateTime(2026, 1, 1));

            Assert.Equal(ProposalStatus.Pending, proposal.Status);
        }

        [Fact]
        public void Should_Start_Proposal_Processing()
        {
            var proposal = CreateValidProposal();

            proposal.StartProcessing();

            Assert.Equal(ProposalStatus.Processing, proposal.Status);
            Assert.Equal(
                ProposalProcessingStep.SimulationValidation,
                proposal.ProcessingStep);
        }

        [Fact]
        public void Should_Not_Complete_Payment_When_Not_In_Payment_Step()
        {
            var proposal = CreateValidProposal();

            proposal.StartProcessing();

            Assert.Throws<DomainException>(
                () => proposal.CompletePayment());
        }

        [Fact]
        public void Should_Reject_Proposal_During_Processing()
        {
            var proposal = CreateValidProposal();

            proposal.StartProcessing();
            proposal.Reject("Risk score below minimum.");

            Assert.Equal(ProposalStatus.Rejected, proposal.Status);
            Assert.Equal(
                "Risk score below minimum.",
                proposal.RejectionReason);
        }

        [Fact]
        public void Should_Complete_Proposal_Processing()
        {
            var proposal = CreateValidProposal();

            proposal.StartProcessing();

            proposal.CompleteSimulationValidation();
            proposal.CompleteRiskAnalysis();
            proposal.CompleteInssRegistration();
            proposal.CompleteContractGeneration();
            proposal.CompleteDigitalSignature();
            proposal.CompletePayment();

            Assert.Equal(ProposalStatus.Approved, proposal.Status);
            Assert.Equal(
                ProposalProcessingStep.Completed,
                proposal.ProcessingStep);
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

        private static Simulation CreateValidSimulation(Proponent proponent)
        {
            return Simulation.Create(
                10_000m,
                48,
                proponent.RetirementIncome);
        }

        private static Proposal CreateValidProposal()
        {
            var proponent = CreateValidProponent();
            var simulation = CreateValidSimulation(proponent);

            var proposal = new Proposal(
                Guid.NewGuid(),
                Guid.NewGuid(),
                proponent,
                simulation,
                new DateTime(2026, 1, 1));

            return proposal;
        }
    }
}
