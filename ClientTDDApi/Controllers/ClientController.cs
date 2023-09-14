using ClientTDDApi.DTOs.Client;
using ClientTDDApi.Interfaces;
using ClientTDDApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClientTDDApi.Controllers
{
    [ApiController]
    [Route("api/clients")]
    public class ClientController : Controller
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet, Authorize(Roles = "worker,admin")]
        public async Task<IActionResult> FindAll()
        {
            IEnumerable<ClientDTO> dtos = await _clientService.FindAllAsync();
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> FindById(string id)
        {
            int entityId = ParseUtils.ParsePathParam(id);
            ClientDTO clientDTO = await _clientService.FindByIdAsync(entityId);
            return Ok(clientDTO);
        }

        [HttpPost, Authorize(Roles = "worker,admin")]
        public async Task<IActionResult> Save([FromBody] ClientInsertDTO clientInsertDTO)
        {
            ClientDTO clientDTO = await _clientService.SaveAsync(clientInsertDTO);
            return Created("/api/clients/" + clientDTO.Id, clientDTO);
        }

        [HttpPut("{id}"), Authorize(Roles = "worker,admin")]
        public async Task<IActionResult> Update([FromBody] ClientUpdateDTO clientUpdateDTO, [FromRoute] string id)
        {
            int entityId = ParseUtils.ParsePathParam(id);
            ClientDTO clientDTO = await _clientService.UpdateAsync(clientUpdateDTO, entityId);
            return Ok(clientDTO);
        }

        [HttpDelete("{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(string id)
        {
            int entityId = ParseUtils.ParsePathParam(id);
            await _clientService.DeleteAsync(entityId);
            return NoContent();
        }
    }
}
