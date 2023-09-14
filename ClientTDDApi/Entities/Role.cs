﻿namespace ClientTDDApi.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public IEnumerable<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
