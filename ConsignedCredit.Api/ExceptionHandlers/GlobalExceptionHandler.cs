using ConsignedCredit.Application.Exceptions;
using ConsignedCredit.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace ConsignedCredit.Api.ExceptionHandlers
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var statusCode = exception switch
            {
                DomainException => StatusCodes.Status400BadRequest,
                BusinessRuleException => StatusCodes.Status422UnprocessableEntity,
                _ => StatusCodes.Status500InternalServerError
            };

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(
                new
                {
                    status = statusCode,
                    error = exception.Message
                },
                cancellationToken);

            return true;
        }
    }
}
