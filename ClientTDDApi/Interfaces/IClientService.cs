using ClientTDDApi.DTOs.Client;

namespace ClientTDDApi.Interfaces
{
    public interface IClientService
    {
        Task<ClientDTO> FindByIdAsync(int id);

        Task<IEnumerable<ClientDTO>> FindAllAsync();

        Task<ClientDTO> SaveAsync(ClientInsertDTO clientInsertDTO);

        Task<ClientDTO> UpdateAsync(ClientUpdateDTO clientUpdateDTO, int id);

        Task DeleteAsync(int id);
    }
}
