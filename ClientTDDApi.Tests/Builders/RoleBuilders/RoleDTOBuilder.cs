using ClientTDDApi.DTOs.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests.Builders.RoleBuilders
{
    public class RoleDTOBuilder
    {
        public static readonly int Id = 3;
        public static readonly string Name = "AdminDTO";

        public static readonly int SecondaryId = 4;
        public static readonly string SecondaryName = "WorkerDTO";

        private RoleDTO _role;

        private RoleDTOBuilder() 
        {
            _role = new RoleDTO()
            {
                Id = Id,
                Name = Name,
            };
        }

        public static RoleDTOBuilder ARoleDTO()
        {
            return new RoleDTOBuilder();
        }

        public RoleDTOBuilder WithSecondaryValues()
        {
            _role = new RoleDTO()
            {
                Id = SecondaryId,
                Name = SecondaryName
            };
            return this;
        }

        public RoleDTO Build()
        {
            return _role;
        }

        public static IEnumerable<RoleDTO> BuildRoleDTOs()
        {
            RoleDTO dto1 = ARoleDTO().Build();
            RoleDTO dto2 = ARoleDTO().WithSecondaryValues().Build();
            return new RoleDTO[] { dto1, dto2 };
        }
    }
}
