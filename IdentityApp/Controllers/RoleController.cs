using IdentityApp.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await roleManager.Roles.ToListAsync();

            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> Create(RoleDto roleDto)
        {
            var identityRole = new IdentityRole
            { 
                Name = roleDto.Name,
                NormalizedName = roleManager.NormalizeKey(roleDto.Name) 
            };

            var result = await roleManager.CreateAsync(identityRole);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return ValidationProblem();
            }

            return StatusCode(201);
        }


        [HttpPut]
        public async Task<IActionResult> Update(RoleUpdateDto roleUpdateDto)
        {
            var identityRole = await roleManager.FindByNameAsync(roleUpdateDto.Name);

            if (identityRole == null) return NotFound();

            identityRole.Name = roleUpdateDto.UpdateName;
            identityRole.NormalizedName = roleManager.NormalizeKey(roleUpdateDto.UpdateName);

            var result = await roleManager.UpdateAsync(identityRole);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return ValidationProblem();
            }

            return StatusCode(201);
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(RoleDto roleDto)
        {
            var identityRole = await roleManager.FindByNameAsync(roleDto.Name);

            if (identityRole == null) return NotFound();

            //ตรวจสอบมีผู้ใช้บทบาทนี้หรือไม่
            var usersInRole = await userManager.GetUsersInRoleAsync(roleDto.Name);
            if (usersInRole.Count != 0) return BadRequest();

            var result = await roleManager.DeleteAsync(identityRole);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return ValidationProblem();
            }

            return StatusCode(201);
        }

    }
}
