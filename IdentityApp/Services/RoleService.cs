using IdentityApp.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Setvices
{
    public class RoleService : ControllerBase, IRoleService
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        public async Task<List<IdentityRole>> GetAsync()
        {
            var result = await roleManager.Roles.ToListAsync();
            return result;
        }

        public async Task<Object> CreateAsync(RoleDto roleDto)
        {
            var identityRole = new IdentityRole
            {
                Name = roleDto.Name,
                NormalizedName = roleManager.NormalizeKey(roleDto.Name)
            };

            var result = await roleManager.CreateAsync(identityRole);

            if (!result.Succeeded) return ResultValidation(result);

            return StatusCode(201);
        }

        public async Task<object> UpdateAsync(RoleUpdateDto roleUpdateDto)
        {
            var identityRole = await roleManager.FindByNameAsync(roleUpdateDto.Name);

            if (identityRole == null) return NotFound();

            identityRole.Name = roleUpdateDto.UpdateName;
            identityRole.NormalizedName = roleManager.NormalizeKey(roleUpdateDto.UpdateName);

            var result = await roleManager.UpdateAsync(identityRole);

            if (!result.Succeeded) return ResultValidation(result);

            return StatusCode(201);
        }

        public async Task<object> DeleteAsync(RoleDto roleDto)
        {
            var identityRole = await roleManager.FindByNameAsync(roleDto.Name);

            if (identityRole == null) return NotFound();

            //ตรวจสอบมีผู้ใช้บทบาทนี้หรือไม่
            var usersInRole = await userManager.GetUsersInRoleAsync(roleDto.Name);
            if (usersInRole.Count != 0) return BadRequest();

            var result = await roleManager.DeleteAsync(identityRole);

            if (!result.Succeeded) return ResultValidation(result);

            return StatusCode(201);
        }

        public Object ResultValidation(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return ValidationProblem();
        }
       
    }
}
