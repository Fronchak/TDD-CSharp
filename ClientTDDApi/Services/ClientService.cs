using ClientTDDApi.DTOs.Client;
using ClientTDDApi.Entities;
using ClientTDDApi.Exceptions;
using ClientTDDApi.Interfaces;

namespace ClientTDDApi.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IClientMapper _clientMapper;

        public ClientService(IClientRepository clientRepository, IClientMapper clientMapper)
        {
            _clientRepository = clientRepository;
            _clientMapper = clientMapper;
        }

        public async Task<ClientDTO> FindByIdAsync(int id)
        {
            Client? client = await _clientRepository.FindByIdAsync(id);
            if(client == null)
            {
                throw new EntityNotFoundException("Client not found");
            }
            return _clientMapper.MapToClientDTO(client);
        }

        public async Task<IEnumerable<ClientDTO>> FindAllAsync()
        {
            IEnumerable<Client> clients = await _clientRepository.FindAllAsync();
            return _clientMapper.MapToClientDTOs(clients);
        }

        public async Task<ClientDTO> SaveAsync(ClientInsertDTO clientInsertDTO)
        {
            Client client = new Client();
            _clientMapper.CopyToClient(clientInsertDTO, client);
            _clientRepository.Save(client);
            await _clientRepository.SaveChangesAsync();
            return _clientMapper.MapToClientDTO(client);
        }

        public async Task<ClientDTO> UpdateAsync(ClientUpdateDTO clientUpdateDTO, int id)
        {
            Client? client = await _clientRepository.FindByIdAsync(id);
            if (client == null)
            {
                throw new EntityNotFoundException("Client not found");
            }
            _clientMapper.CopyToClient(clientUpdateDTO, client);
            _clientRepository.Update(client);
            await _clientRepository.SaveChangesAsync();
            return _clientMapper.MapToClientDTO(client);
        }

        public async Task DeleteAsync(int id)
        {
            Client? client = await _clientRepository.FindByIdAsync(id);
            if (client == null)
            {
                throw new EntityNotFoundException("Client not found");
            }
            _clientRepository.Delete(client);
            await _clientRepository.SaveChangesAsync();
        }
    }
}
