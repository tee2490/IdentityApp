using IdentityApp.DTOs;
using IdentityApp.Setvices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly TokenService tokenService;
        private readonly UserManager<ApplicationUser> userManager;

        public AccountController(TokenService tokenService, UserManager<ApplicationUser> userManager)
        {
            this.tokenService = tokenService;
            this.userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await userManager.Users.ToListAsync();

            List<Object> users = new();

            foreach (var user in result)
            {
                var userRole = await userManager.GetRolesAsync(user);
                users.Add(new { user.UserName, userRole });
            }

            return Ok(users);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await userManager.FindByNameAsync(loginDto.Username);

            if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
                return Unauthorized();

            var userDto = new UserDto
            {
                Email = user.Email,
                Token = await tokenService.GenerateToken(user),
            };

            return Ok(userDto);
        }


        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto registerDto)
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

        [HttpGet("TestAdminRole"), Authorize(Roles = "Admin")]
        public IActionResult test()
        {
            return Ok("Authorize Success");
        }


        [HttpGet("GetMeInBaseController"), Authorize]
        public async Task<IActionResult> GetMyName()
        {
            //var userName = User.FindFirstValue(ClaimTypes.Name);
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            var roles = await userManager.GetRolesAsync(user);

            return Ok(new { user.UserName, roles });
        }


        [HttpGet("GetToken"), Authorize]
        public async Task<IActionResult> GetToken()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                accessToken = "Not Login";
            }

            return Ok(accessToken);
        }

    }
}
