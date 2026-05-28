using Forge.Application.Common;
using Forge.Application.Features.Auth.Commands;
using Forge.Application.Features.Auth.Requests;
using Forge.Common.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Desk.WebApi.Controllers
{
    [AllowAnonymous]
    public class AuthController : ForgeBaseController
    {
        private readonly ISender _mediator;

        public AuthController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new LoginCommand(request), cancellationToken);

            return result.Status switch
            {
                OperationStatus.Ok => Ok(result.Value),
                OperationStatus.BadRequest => BadRequest(new { message = result.Message, errors = result.Errors }),
                OperationStatus.Unauthorized => Unauthorized(new LoginResponse { Success = false, Message = result.Message }),
                _ => StatusCode(500)
            };
        }
    }
}
