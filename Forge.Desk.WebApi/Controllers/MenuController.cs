using Forge.Desk.WebApi.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forge.Desk.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/menu")]
    public class MenuController : ControllerBase
    {
        private readonly IMenuProvider _menuProvider;

        public MenuController(IMenuProvider menuProvider)
        {
            _menuProvider = menuProvider;
        }

        [HttpGet]
        public ActionResult<IReadOnlyList<MenuItem>> Get()
        {
            return Ok(_menuProvider.GetMenu());
        }
    }
}
