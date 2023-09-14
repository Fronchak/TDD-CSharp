using ClientTDDApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests.Builders.RoleBuilders
{
    public class RoleBuilder
    {
        public static readonly int Id = 1;
        public static readonly string Name = "Admin";

        public static readonly int SecondaryId = 2;
        public static readonly string SecondaryName = "Worker";

        private Role _role;

        private RoleBuilder()
        {
            _role = new Role()
            {
                Id = Id,
                Name = Name,
            };
        }

        public static RoleBuilder ARole()
        {
            return new RoleBuilder();
        }

        public RoleBuilder WithSecondaryValues()
        {
            _role = new Role()
            {
                Id = SecondaryId,
                Name = SecondaryName
            };
            return this;
        }

        public Role Build()
        {
            return _role;
        }

        public static IEnumerable<Role> BuildRoles() 
        { 
            Role role1 = ARole().Build();
            Role role2 = ARole().WithSecondaryValues().Build();
            return new Role[] { role1, role2 };
        }
    }
}
