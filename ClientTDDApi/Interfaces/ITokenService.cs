using ClientTDDApi.Entities;

namespace ClientTDDApi.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
