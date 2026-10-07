namespace ConsignedCredit.Application.Proposals.Events;

public sealed record ProposalCreatedEvent(
    Guid ProposalId);
