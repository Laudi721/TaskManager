using Forge.Application.Features.Menu.Queries;
using Forge.Common.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Desk.WebApi.Controllers
{
    public class MenuController : ForgeBaseController
    {
        private readonly ISender _mediator;

        public MenuController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<MenuItem>>> Get(CancellationToken cancellationToken)
        {
            var menu = await _mediator.Send(new GetMenuQuery(), cancellationToken);

            return Ok(menu);
        }
    }
}
