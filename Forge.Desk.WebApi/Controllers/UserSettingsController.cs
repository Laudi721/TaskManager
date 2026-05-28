using Forge.Application.Features.UserSettings.Commands;
using Forge.Application.Features.UserSettings.Queries;
using Forge.Application.Features.UserSettings.Requests;
using Forge.Common.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Desk.WebApi.Controllers
{
    /// <summary>
    /// Preferencje per-user (motyw, w przyszłości język/density/itp.). Wszystkie endpointy
    /// dotyczą obecnie zalogowanego użytkownika — userId czytamy z JWT, klient nie może
    /// wskazać innego konta.
    /// </summary>
    public class UserSettingsController : ForgeBaseController
    {
        private readonly ISender _mediator;

        public UserSettingsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<UserPreferencesDto>> GetMine(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new GetMyPreferencesQuery(userId), cancellationToken);

            return result.Status switch
            {
                Application.Common.OperationStatus.Ok => Ok(result.Value),
                Application.Common.OperationStatus.NotFound => NotFound(),
                _ => StatusCode(500)
            };
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMine(
            [FromBody] UserPreferencesUpdateRequest dto,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new UpdateMyPreferencesCommand(userId, dto), cancellationToken);

            return MapToResponse(result);
        }
    }
}
