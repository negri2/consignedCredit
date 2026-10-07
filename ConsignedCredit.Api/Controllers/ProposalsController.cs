using ConsignedCredit.Application.Proposals.Create;
using ConsignedCredit.Application.Proposals.Get;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
            var result = await useCase.ExecuteAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.ProposalId },
                result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            [FromServices] GetProposalUseCase useCase,
            CancellationToken cancellationToken)
        {
            var result = await useCase.ExecuteAsync(
                id,
                cancellationToken);

            if (result is null)
                return NotFound();

            return Ok(result);
        }
    }
}
