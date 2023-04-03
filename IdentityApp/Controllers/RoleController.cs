using IdentityApp.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService roleService;

        public RoleController(IRoleService roleService)
        {
            this.roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await roleService.GetAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create(RoleDto roleDto)
        {
            var result = await roleService.CreateAsync(roleDto);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(RoleUpdateDto roleUpdateDto)
        {
            var result = await roleService.UpdateAsync(roleUpdateDto);
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(RoleDto roleDto)
        {
            var result = await roleService.DeleteAsync(roleDto);
            return Ok(result);
        }

    }
}
