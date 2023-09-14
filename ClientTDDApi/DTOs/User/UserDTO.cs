using ClientTDDApi.DTOs.Role;

namespace ClientTDDApi.DTOs.User
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public IEnumerable<RoleDTO> Roles { get; set; } = new List<RoleDTO>();

        public UserDTO() { }

        public UserDTO(int id, string email)
        {
            Id = id;
            Email = email;
        }
    }
}
