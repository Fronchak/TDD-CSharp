using ClientTDDApi.Interfaces;

namespace ClientTDDApi.Services
{
    public class BCryptPasswordEncoder : IPasswordEncoder
    {
        public string GenerateSalt()
        {
            return BCrypt.Net.BCrypt.GenerateSalt();
        }

        public string HashPassword(string rawPassword, string salt)
        {
            return BCrypt.Net.BCrypt.HashPassword(rawPassword, salt);
        }

        public bool VerifyPassword(string rawPassword, string hashed)
        {
            return BCrypt.Net.BCrypt.Verify(rawPassword, hashed);
        }
    }
}
