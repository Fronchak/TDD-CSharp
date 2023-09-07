using ClientTDDApi.DTOs.Client;
using ClientTDDApi.Entities;
using ClientTDDApi.Interfaces;

namespace ClientTDDApi.Mappers
{
    public class ClientMapper : IClientMapper
    {
        public ClientDTO MapToClientDTO(Client client)
        {
            return new ClientDTO(client.Id, client.Name, client.Email);
        }

        public IEnumerable<ClientDTO> MapToClientDTOs(IEnumerable<Client> clients)
        {
            return clients.ToList()
                .Select((client) => MapToClientDTO(client));
        }

        public void CopyToClient(ClientInputDTO clientInputDTO, Client client) 
        { 
            client.Name = clientInputDTO.Name;
            client.Email = clientInputDTO.Email;
        }
    }
}
