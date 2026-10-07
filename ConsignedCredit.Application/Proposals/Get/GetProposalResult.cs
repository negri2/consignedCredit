using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsignedCredit.Application.Proposals.Get
{
    public sealed record GetProposalResult(
        Guid ProposalId,
        string Cpf,
        decimal RequestedAmount,
        int Installments,
        decimal InstallmentAmount,
        string Status,
        string ProcessingStep,
        string? RejectionReason,
        DateTime CreatedAt);
}
