using ClientTDDApi.DTOs.Auth;

namespace ClientTDDApi.Interfaces
{
    public interface IAuthService
    {
        Task Register(UserRegisterDTO userRegisterDTO);

        Task<TokenDTO> Login(LoginDTO loginDTO);
    }
}
