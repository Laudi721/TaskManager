using Forge.Application.Features.Roles.Commands;
using Forge.Application.Features.Roles.Queries;
using Forge.Application.Features.Roles.Requests;
using Forge.Common.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Desk.WebApi.Controllers
{
    public class RolesController : ForgeBaseController
    {
        private readonly ISender _mediator;

        public RolesController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<RoleDto>>> Get(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetRolesQuery(), cancellationToken);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoleCreateRequest dto, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CreateRoleCommand(dto), cancellationToken);

            return MapToResponse(result, successStatusCode: StatusCodes.Status201Created);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteRoleCommand(id), cancellationToken);

            return MapToResponse(result, successStatusCode: StatusCodes.Status201Created);
        }

        [HttpPost("archive/{id:int}")]
        public async Task<IActionResult> ToggleArchive(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ToggleRoleArchiveCommand(id), cancellationToken);

            return MapToResponse(result);
        }
    }
}
