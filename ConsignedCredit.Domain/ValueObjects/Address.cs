using ConsignedCredit.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Domain.ValueObjects
{
    public sealed class Address
    {
        public string Street { get; private set; }
        public string Number { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string ZipCode { get; private set; }

        private Address() { }

        public Address(
            string street,
            string number,
            string city,
            string state,
            string zipCode)
        {
            if (string.IsNullOrWhiteSpace(street))
                throw new DomainException("Street is required.");

            if (string.IsNullOrWhiteSpace(city))
                throw new DomainException("City is required.");

            if (string.IsNullOrWhiteSpace(state))
                throw new DomainException("State is required.");

            Street = street;
            Number = number;
            City = city;
            State = state;
            ZipCode = zipCode;
        }
    }
}
