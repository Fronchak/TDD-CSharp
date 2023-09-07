namespace ClientTDDApi.DTOs.Client
{
    public class ClientDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public ClientDTO() { }

        public ClientDTO(int id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
        }
    }
}
