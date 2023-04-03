using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityApp.Setvices
{
    public class TokenService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;
        public TokenService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            this.configuration = configuration;
            this.userManager = userManager;
        }

        public async Task<string> GenerateToken(ApplicationUser user)
        {
            //Claim คือข้อมูลที่เราต้องการนำมาเก็บไว้ในตั๋ว สำหรับใช้ยืนยันตัวตน
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var roles = await userManager.GetRolesAsync(user);

            //กรณีมีหลาย roles
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            //อ่านค่ารหัสลับ และกำหนดอัลกอริทึมการเข้ารหัส
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTSettings:TokenKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            //รวบรวมค่าต่างๆ สำหรับบรรจุไว้ใน container Token
            var tokenOptions = new JwtSecurityToken
            (
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            //สร้าง Token
            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }
    }

}
