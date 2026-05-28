using Forge.Application.Features.Users.Commands;
using Forge.Application.Features.Users.Queries;
using Forge.Application.Features.Users.Requests;
using Forge.Common.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Desk.WebApi.Controllers
{
    public class UsersController : ForgeBaseController
    {
        private readonly ISender _mediator;

        public UsersController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<UserDto>>> Get(CancellationToken cancellationToken)
        {
            var users = await _mediator.Send(new GetUsersQuery(), cancellationToken);

            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserCreateRequest dto, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CreateUserCommand(dto), cancellationToken);

            return MapToResponse(result, successStatusCode: StatusCodes.Status201Created);
        }

        [HttpPost("archive/{id:int}")]
        public async Task<IActionResult> ToggleArchive(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ToggleUserArchiveCommand(id, GetCurrentUserId()), cancellationToken);

            return MapToResponse(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteUserCommand(id, GetCurrentUserId()), cancellationToken);

            return MapToResponse(result);
        }
    }
}
