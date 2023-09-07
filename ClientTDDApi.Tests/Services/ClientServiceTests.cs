using ClientTDDApi.DTOs.Client;
using ClientTDDApi.Entities;
using ClientTDDApi.Exceptions;
using ClientTDDApi.Interfaces;
using ClientTDDApi.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests.Services
{
    public class ClientServiceTests
    {
        private readonly Mock<IClientRepository> _clientRepository;
        private readonly Mock<IClientMapper> _clientMapper;
        private readonly ClientService _clientService;

        private readonly int ExistingId;
        private readonly int NonExistingId;

        private Client? Client;
        private Client? ClientNull;
        private ClientDTO ClientDTO;

        public ClientServiceTests()
        {
            _clientRepository = new Mock<IClientRepository>();
            _clientMapper = new Mock<IClientMapper>();
            _clientService = new ClientService(_clientRepository.Object, _clientMapper.Object);

            ExistingId = 1;
            NonExistingId = 2;

            Client = new Client();
            ClientNull = null;
            ClientDTO = new ClientDTO();

            _clientRepository.Setup((r) => r.FindByIdAsync(NonExistingId)).ReturnsAsync(ClientNull);
            _clientRepository.Setup((r) => r.FindByIdAsync(ExistingId)).ReturnsAsync(Client);

            _clientMapper.Setup((m) => m.MapToClientDTO(Client)).Returns(ClientDTO);
        }

        [Fact]
        public async Task FindByIdShouldThrowEntityNotFoundExceptionWhenIdDoesNotExist()
        {
            Func<Task> action = async () => await _clientService.FindByIdAsync(NonExistingId);

            await action.Should().ThrowAsync<EntityNotFoundException>();
        }

        [Fact]
        public async Task FindByIdShouldReturnClientDTOWhenIdExists()
        {
            ClientDTO result = await _clientService.FindByIdAsync(ExistingId);

            result.Should().Be(ClientDTO);
        }

        [Fact]
        public async Task FindAllShouldReturnIEnumerable()
        {
            Client OtherClient = new Client();
            ClientDTO OtherClientDTO = new ClientDTO();
            IEnumerable<Client> clients = new List<Client>() { Client, OtherClient };
            IEnumerable<ClientDTO> clientDTOs = new List<ClientDTO>() { ClientDTO, OtherClientDTO };
            _clientRepository.Setup((r) => r.FindAllAsync()).ReturnsAsync(clients);
            _clientMapper.Setup((m) => m.MapToClientDTOs(clients)).Returns(clientDTOs);

            IEnumerable<ClientDTO> result = await _clientService.FindAllAsync();

            result.Should().BeSameAs(clientDTOs);
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task SaveShouldSaveInTheDatabaseAndReturnClientDTO()
        {
            ClientInsertDTO clientInsertDTO = new ClientInsertDTO();
            ClientDTO otherClientDTO = new ClientDTO();
            _clientMapper.Setup((m) => m.MapToClientDTO(It.IsAny<Client>())).Returns(otherClientDTO);

            ClientDTO result = await _clientService.SaveAsync(clientInsertDTO);

            _clientMapper.Verify((m) => m.CopyToClient(clientInsertDTO, It.IsAny<Client>()), Times.Once());
            _clientRepository.Verify((r) => r.Save(It.IsAny<Client>()), Times.Once());
            _clientRepository.Verify((r) => r.SaveChangesAsync(), Times.Once());
            _clientMapper.Verify((m) => m.MapToClientDTO(It.IsAny<Client>()), Times.Once());
            result.Should().Be(otherClientDTO);
        }

        [Fact]
        public async Task UpdateShouldThrowEntityNotFoundExceptionWhenIdDoesNotExists()
        {
            Func<Task> action = async () => await _clientService.UpdateAsync(new ClientUpdateDTO(), NonExistingId);

            await action.Should().ThrowAsync<EntityNotFoundException>();
            _clientRepository.Verify((r) => r.Update(It.IsAny<Client>()), Times.Never());
            _clientRepository.Verify((r) => r.SaveChangesAsync(), Times.Never());
        }

        [Fact]
        public async Task UpdateShouldUpdateDatabaseAndReturnClientDTOWhenIdExsits()
        {
            ClientUpdateDTO clientUpdateDTO = new ClientUpdateDTO();

            ClientDTO result = await _clientService.UpdateAsync(clientUpdateDTO, ExistingId);

            _clientMapper.Verify((m) => m.CopyToClient(clientUpdateDTO, Client), Times.Once());
            _clientRepository.Verify((r) => r.Update(Client), Times.Once());
            _clientRepository.Verify((r) => r.SaveChangesAsync(), Times.Once());
            _clientMapper.Verify((m) => m.MapToClientDTO(Client), Times.Once());
            result.Should().Be(ClientDTO);
        }

        [Fact]
        public async Task DeleteShouldThrowEntityNotFoundExceptionWhenIdDoesNotExit()
        {
            Func<Task> action = async () => await _clientService.DeleteAsync(NonExistingId);

            await action.Should().ThrowAsync<EntityNotFoundException>();
            _clientRepository.Verify((r) => r.Delete(It.IsAny<Client>()), Times.Never());
            _clientRepository.Verify((r) => r.SaveChangesAsync(), Times.Never());
        }

        [Fact]
        public async Task DeleteShouldRemoveFromDatabaseEmIdExists()
        {
            await _clientService.DeleteAsync(ExistingId);

            _clientRepository.Verify((r) => r.Delete(Client), Times.Once());
            _clientRepository.Verify((r) => r.SaveChangesAsync(), Times.Once());
        }
    }
}
