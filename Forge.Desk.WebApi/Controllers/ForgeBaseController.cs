using System.Security.Claims;
using Forge.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Desk.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public abstract class ForgeBaseController : ControllerBase
    {
        protected int GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(sub, out var id) ? id : 0;
        }

        protected IActionResult MapToResponse(
            OperationResult<Unit> result,
            int successStatusCode = StatusCodes.Status204NoContent)
        {
            return result.Status switch
            {
                OperationStatus.Ok => successStatusCode == StatusCodes.Status204NoContent
                    ? NoContent()
                    : StatusCode(successStatusCode, new { message = result.Message }),
                OperationStatus.BadRequest => BadRequest(new { message = result.Message, errors = result.Errors }),
                OperationStatus.Unauthorized => Unauthorized(new { message = result.Message }),
                OperationStatus.NotFound => NotFound(),
                OperationStatus.Conflict => Conflict(new { message = result.Message }),
                _ => StatusCode(500)
            };
        }
    }
}
