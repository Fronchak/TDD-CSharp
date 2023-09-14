namespace ClientTDDApi.Interfaces
{
    public interface IPasswordEncoder
    {
        string GenerateSalt();

        string HashPassword(string rawPassword, string salt);

        bool VerifyPassword(string rawPassword, string hashed);
    }
}
