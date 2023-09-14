namespace ClientTDDApi.DTOs.User
{
    public class UserSimpleDTO
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;

        public UserSimpleDTO() { }

        public UserSimpleDTO(int id, string email)
        {
            Id = id;
            Email = email;
        }
    }
}
