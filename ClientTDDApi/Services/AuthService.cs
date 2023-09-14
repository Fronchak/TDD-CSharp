using ClientTDDApi.DTOs.Auth;
using ClientTDDApi.Entities;
using ClientTDDApi.Exceptions;
using ClientTDDApi.Interfaces;

namespace ClientTDDApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordEncoder _passwordEncoder;
        private readonly ITokenService _tokenService;

        public AuthService(IUserRepository userRepository, IPasswordEncoder passwordEncoder, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _passwordEncoder = passwordEncoder;
            _tokenService = tokenService;
        }

        public async Task Register(UserRegisterDTO userRegisterDTO)
        {
            string salt = _passwordEncoder.GenerateSalt();
            string hashed = _passwordEncoder.HashPassword(userRegisterDTO.Password, salt);

            User user = new User()
            {
                Email = userRegisterDTO.Email!,
                Password = hashed
            };

            _userRepository.Save(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task<TokenDTO> Login(LoginDTO loginDTO)
        {
            User? user = _userRepository.FindByEmail(loginDTO.Email!);
            if(user == null)
            {
                throw new UnauthorizedException("Email or password invalid");
            }
            bool passwordMatch = _passwordEncoder.VerifyPassword(loginDTO.Password, user.Password);
            if(!passwordMatch)
            {
                throw new UnauthorizedException("Email or password invalid");
            }
            user = await _userRepository.FindByIdAsync(user.Id);
            string token = _tokenService.GenerateToken(user!);
            TokenDTO tokenDTO = new TokenDTO() { Access_Token = token };
            return tokenDTO;
        }
    }
}
