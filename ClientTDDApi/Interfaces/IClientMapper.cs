using ClientTDDApi.DTOs.Client;
using ClientTDDApi.Entities;

namespace ClientTDDApi.Interfaces
{
    public interface IClientMapper
    {
        ClientDTO MapToClientDTO(Client client);

        IEnumerable<ClientDTO> MapToClientDTOs(IEnumerable<Client> clients);

        void CopyToClient(ClientInputDTO clientInputDTO, Client client);
    }
}
