using IdentityApp.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityApp.Setvices
{
    public class AccountService : ControllerBase, IAccountService
    {

        private readonly UserManager<ApplicationUser> userManager;
        private readonly TokenService tokenService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public AccountService(UserManager<ApplicationUser> userManager, TokenService tokenService, IHttpContextAccessor httpContextAccessor)
        {
            this.userManager = userManager;
            this.tokenService = tokenService;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<Object>> GetUsersAsync()
        {
            // var result = await _userManager.Users.Select(x=>x.UserName).ToListAsync();

            var result = await userManager.Users.ToListAsync();

            List<Object> users = new();

            foreach (var user in result)
            {
                var userRole = await userManager.GetRolesAsync(user);
                users.Add(new { user.UserName, userRole });
            }

            return users;
        }

        public async Task<UserDto> LoginAsync(LoginDto loginDto)
        {
            var user = await userManager.FindByNameAsync(loginDto.Username);

            if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
                return null;

            var userDto = new UserDto
            {
                Email = user.Email,
                Token = await tokenService.GenerateToken(user),
            };

            return userDto;
        }

        public async Task<object> RegisterAsync(RegisterDto registerDto)
        {
            var user = new ApplicationUser { UserName = registerDto.Username, Email = registerDto.Email };
            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return ValidationProblem();
            }

            await userManager.AddToRoleAsync(user, registerDto.Role);
            return StatusCode(201);
        }

        public Object GetMe()
        {
            var username = string.Empty;
            var role = string.Empty;

            if (httpContextAccessor.HttpContext != null)
            {
                username = httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
                role = httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
            }
            return new { username, role };
        }

    }
}
