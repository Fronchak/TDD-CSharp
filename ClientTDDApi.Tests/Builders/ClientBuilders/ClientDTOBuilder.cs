using ClientTDDApi.DTOs.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests.Builders.ClientBuilders
{
    public class ClientDTOBuilder
    {
        public static readonly int Id = 10;
        public static readonly string Name = "Maria";
        public static readonly string Email = "maria@gmail.com";

        public static readonly int SecondaryId = 11;
        public static readonly string SecondaryName = "João";
        public static readonly string SecondaryEmail = "joao@gmail.com";

        private ClientDTO _client;

        private ClientDTOBuilder() 
        {
            _client = new ClientDTO()
            {
                Id = Id,
                Name = Name,
                Email = Email
            };
        }

        public static ClientDTOBuilder aClientDTO()
        {
            return new ClientDTOBuilder();
        }

        public ClientDTOBuilder WithSecondaryValues()
        {
            _client = new ClientDTO()
            {
                Id = SecondaryId,
                Name = SecondaryName,
                Email = SecondaryEmail
            };
            return this;
        }

        public ClientDTO Build()
        {
            return _client;
        }

        public static IEnumerable<ClientDTO> BuildClientDTOs()
        {
            ClientDTO clientDTO1 = aClientDTO().Build();
            ClientDTO clientDTO2 = aClientDTO().WithSecondaryValues().Build();
            return new List<ClientDTO>() { clientDTO1, clientDTO2 };
        }
    }
}
