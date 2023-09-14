using ClientTDDApi.DTOs.Client;
using ClientTDDApi.Entities;
using ClientTDDApi.Mappers;
using ClientTDDApi.Tests.Builders.ClientBuilders;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests.Mappers
{
    public class ClientMapperTests
    {
        private readonly ClientMapper _clientMapper;

        private readonly int ClientId;
        private readonly string ClientName;
        private readonly string ClientEmail;

        private readonly int OtherClientId;
        private readonly string OtherClientName;
        private readonly string OtherClientEmail;

        private Client Client;
        private Client OtherClient;

        public ClientMapperTests()
        {
            ClientId = 1;
            ClientName = "Ana";
            ClientEmail = "ana@gmail.com";
            Client = ClientBuilder.Create()
                .WithId(ClientId)
                .WithName(ClientName)
                .WithEmail(ClientEmail)
                .Get();

            OtherClientId = 2;
            OtherClientName = "Fernando";
            OtherClientEmail = "fernando@gmail.com";
            OtherClient = ClientBuilder.Create()
                .WithId(OtherClientId)
                .WithName(OtherClientName)
                .WithEmail(OtherClientEmail)
                .Get();

            _clientMapper = new ClientMapper();
        }

        [Fact]
        public void MapToClientDTOShouldReturnClientDTO()
        {
            ClientDTO result = _clientMapper.MapToClientDTO(Client);

            result.Id.Should().Be(ClientId);
            result.Name.Should().Be(ClientName);
            result.Email.Should().Be(ClientEmail);
        }

        [Fact]
        public void MapToClientDTOsShouldReturnIEnumarable()
        {
            IEnumerable<Client> clients = new List<Client>() { Client, OtherClient };

            IEnumerable<ClientDTO> result = _clientMapper.MapToClientDTOs(clients);

            result.Should()
                .HaveCount(2)
                .And
                .Contain((clientDTO) =>
                    clientDTO.Id.Equals(ClientId) &&
                    clientDTO.Name.Equals(ClientName) &&
                    clientDTO.Email.Equals(ClientEmail)
                )
                .And
                .Contain((clientDTO) =>
                    clientDTO.Id.Equals(OtherClientId) &&
                    clientDTO.Name.Equals(OtherClientName) &&
                    clientDTO.Email.Equals(OtherClientEmail)
                );
        }

        [Fact]
        public void CopyToClientShouldCopyValuesToEntity()
        {
            string newName = "Carlos";
            string newEmail = "carlos@gmail.com";
            ClientInputDTO clientInputDTO = new ClientInsertDTO()
            {
                Name = newName,
                Email = newEmail
            };

            _clientMapper.CopyToClient(clientInputDTO, Client);

            Client.Id.Should().Be(ClientId);
            Client.Name.Should().Be(newName);
            Client.Email.Should().Be(newEmail);
        }
    }
}
