using IdentityApp.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService accountService;
        private readonly UserManager<ApplicationUser> userManager;

        public AccountController(IAccountService accountService, UserManager<ApplicationUser> userManager)
        {
            this.accountService = accountService;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var users = await accountService.GetUsersAsync();
            return Ok(users);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await accountService.LoginAsync(loginDto);

            if (user == null) return Unauthorized();

            return Ok(user);

        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto registerDto)
        {
            var result = await accountService.RegisterAsync(registerDto);
            return Ok(result);
        }

        [HttpGet("TestAdminRole"), Authorize(Roles = "Admin")]
        public IActionResult test()
        {
            return Ok("Authorize Success");
        }

        [HttpGet("GetMeByContext"), Authorize]
        public IActionResult GetMe()
        {
            var result = accountService.GetMe();
            return Ok(result);
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
