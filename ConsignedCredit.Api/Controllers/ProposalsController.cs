using ConsignedCredit.Application.Proposals.Create;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConsignedCredit.Api.Controllers
{
    [ApiController]
    [Route("api/proposals")]
    public sealed class ProposalsController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateProposalRequest request,
            [FromServices] CreateProposalUseCase useCase,
            CancellationToken cancellationToken)
        {
            var proposalId = await useCase.ExecuteAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = proposalId },
                new { id = proposalId });
        }

        [HttpGet("{id:guid}")]
        public IActionResult GetById(Guid id)
        {
            return Ok();
        }
    }
}
