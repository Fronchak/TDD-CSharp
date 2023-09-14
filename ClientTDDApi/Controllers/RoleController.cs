using ClientTDDApi.DTOs.Role;
using ClientTDDApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClientTDDApi.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RoleController : Controller
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> FindAll()
        {
            IEnumerable<RoleDTO> dtos = await _roleService.FindAllAsync();
            return Ok(dtos);
        }
    }
}
