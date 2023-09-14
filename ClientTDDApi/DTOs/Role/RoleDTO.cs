namespace ClientTDDApi.DTOs.Role
{
    public class RoleDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public RoleDTO() { }

        public RoleDTO(int id, string name)
        {
            Id = id;    
            Name = name;
        }
    }
}
