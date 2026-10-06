using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Application.Proposals.Create
{
    public sealed record CreateProposalRequest(
        Guid AgentId,
        Guid StoreId,
        string Cpf,
        string InssNumber,
        decimal RetirementIncome,
        DateOnly BirthDate,
        string Email,
        string Phone,
        string Street,
        string Number,
        string City,
        string State,
        string ZipCode,
        decimal RequestedAmount,
        int Installments);
}
