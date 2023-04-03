using IdentityApp.DTOs;

namespace IdentityApp.Setvices.IService
{
    public interface IAccountService
    {
        Task<List<Object>> GetUsersAsync();
        Task<UserDto> LoginAsync(LoginDto loginDto);
        Task<Object> RegisterAsync(RegisterDto registerDto);
        Object GetMe();
    }
}
