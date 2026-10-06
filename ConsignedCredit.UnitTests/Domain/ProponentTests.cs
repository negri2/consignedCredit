using ConsignedCredit.Domain.Entities;
using ConsignedCredit.Domain.Exceptions;
using ConsignedCredit.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.UnitTests.Domain
{
    public class ProponentTests
    {
        private readonly Address _validAddress = new(
            "Rua Teste",
            "123",
            "Caxias do Sul",
            "RS",
            "95000-000");

        [Fact]
        public void Should_Create_Proponent_When_Data_Is_Valid()
        {
            var proponent = new Proponent(
                "12345678901",
                "123456789",
                3000m,
                new DateOnly(1960, 1, 1),
                "test@test.com",
                "54999999999",
                _validAddress);

            Assert.Equal("12345678901", proponent.Cpf);
            Assert.Equal("123456789", proponent.InssNumber);
            Assert.Equal(3000m, proponent.RetirementIncome);
            Assert.Equal(_validAddress, proponent.Address);
        }

        [Fact]
        public void Should_Throw_When_Cpf_Is_Empty()
        {
            var act = () => new Proponent(
                "",
                "123456789",
                3000m,
                new DateOnly(1960, 1, 1),
                "test@test.com",
                "54999999999",
                _validAddress);

            var exception = Assert.Throws<DomainException>(act);

            Assert.Equal("CPF is required.", exception.Message);
        }

        [Fact]
        public void Should_Throw_When_Retirement_Income_Is_Not_Positive()
        {
            var act = () => new Proponent(
                "12345678901",
                "123456789",
                0,
                new DateOnly(1960, 1, 1),
                "test@test.com",
                "54999999999",
                _validAddress);

            var exception = Assert.Throws<DomainException>(act);

            Assert.Equal(
                "Retirement income must be greater than zero.",
                exception.Message);
        }

        [Fact]
        public void Should_Throw_When_Address_Is_Null()
        {
            var act = () => new Proponent(
                "12345678901",
                "123456789",
                3000m,
                new DateOnly(1960, 1, 1),
                "test@test.com",
                "54999999999",
                null!);

            var exception = Assert.Throws<DomainException>(act);

            Assert.Equal("Address is required.", exception.Message);
        }
    }
}
