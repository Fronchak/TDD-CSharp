using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Json;
using ClientTDDApi.DTOs.Client;
using ClientTDDApi.Interfaces;
using ClientTDDApi.Tests.Builders.ClientBuilders;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ClientTDDApi.Exceptions;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc;
using ClientTDDApi.Entities;
using Microsoft.AspNetCore.Authentication;
using ClientTDDApi.Tests.Util;

namespace ClientTDDApi.Tests.Controllers
{
    public class ClientControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Mock<IClientService> _clientService;
        private readonly Mock<IClientRepository> _clientRepository;
        private readonly HttpClient _client;

        private readonly int ExistingId;
        private readonly int NonExistingId;
        private readonly int ExistingEmailId;

        public ClientControllerTests(WebApplicationFactory<Program> factory)
        {
            ExistingId = 1;
            NonExistingId = 2;
            ExistingEmailId = 3;

            _clientService = new Mock<IClientService>();
            _clientRepository = new Mock<IClientRepository>();
            _factory = factory.WithWebHostBuilder((builder) =>
            {
                builder.ConfigureServices((services) =>
                {
                    services.AddAuthentication("Test");
                    services.AddScoped((sp) => _clientService.Object);
                    services.AddScoped((sp) => _clientRepository.Object);
                    services.AddTransient<IAuthenticationSchemeProvider, MockSchemeProvider>();

                });
            });
            
            _client = _factory.CreateClient();

            ClientDTO clientDTOResponse = ClientDTOBuilder.aClientDTO().Build();
            _clientService.Setup((service) => service.FindAllAsync()).ReturnsAsync(ClientDTOBuilder.BuildClientDTOs());
            _clientService.Setup((service) => service.FindByIdAsync(NonExistingId)).ThrowsAsync(new EntityNotFoundException("Not found"));
            _clientService.Setup((service) => service.FindByIdAsync(ExistingId)).ReturnsAsync(clientDTOResponse);
            _clientService.Setup((service) => service.SaveAsync(It.IsAny<ClientInsertDTO>())).ReturnsAsync(ClientDTOBuilder.aClientDTO().Build());
            _clientService.Setup((service) => service.UpdateAsync(It.IsAny<ClientUpdateDTO>(), NonExistingId)).ThrowsAsync(new EntityNotFoundException("Not found for update"));
            _clientService.Setup((service) => service.UpdateAsync(
                It.IsAny<ClientUpdateDTO>(),
                It.Is<int>((id) => id == ExistingId || id == ExistingEmailId))
            ).ReturnsAsync(clientDTOResponse);
            _clientService.Setup((service) => service.DeleteAsync(NonExistingId)).ThrowsAsync(new EntityNotFoundException("Not found for delete"));

            string registeredEmail = ClientInsertDTOBuilder.RegisteredEmail;
            Client? clientNull = null;
            _clientRepository.Setup((r) => r.FindByEmail(It.Is<string>((email) => email != registeredEmail))).Returns(clientNull);
            _clientRepository.Setup((r) => r.FindByEmail(registeredEmail)).Returns(ClientBuilder.Create().WithId(ExistingEmailId).Get());
        }

        [Fact]
        public async Task FindAllShouldReturnClientsList()
        {
            HttpResponseMessage? response = await _client.GetAsync("/api/clients");
            //IEnumerable<ClientDTO>? result = await response.Content.ReadFromJsonAsync<IEnumerable<ClientDTO>>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            //result.Should().NotBeNull();
            //result.Count().Should().Be(2);
            _clientService.Verify((service) => service.FindAllAsync(), Times.Once());
        }

        [Fact]
        public async Task FindAllShouldReturnServerErrorWhenSomethingGoesWrong()
        {
            _clientService.Setup((s) => s.FindAllAsync()).ThrowsAsync(new Exception());

            HttpResponseMessage? response = await _client.GetAsync("/api/clients");
            ExceptionResponse? exceptionResponse = await response.Content.ReadFromJsonAsync<ExceptionResponse>();

            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            exceptionResponse.Should().NotBeNull();
            exceptionResponse.Message.Should().Be("Something went wrong");
        }

        [Fact]
        public async Task FindByIdShouldReturnBadRequestWhenIdIsNotANumber()
        {
            HttpResponseMessage? response = await _client.GetAsync("/api/clients/A");
            ExceptionResponse? exceptionResponse = await response.Content.ReadFromJsonAsync<ExceptionResponse>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exceptionResponse.Should().NotBeNull();
            exceptionResponse.Message.Should().Be("Id must be a number");
            _clientService.Verify((service) => service.FindByIdAsync(It.IsAny<int>()), Times.Never());
        }

        [Fact]
        public async Task FindByIdShouldReturnNotFoundWhenIdDoesNotExist()
        {
            HttpResponseMessage? response = await _client.GetAsync("/api/clients/" + NonExistingId);
            ExceptionResponse? exceptionResponse = await response.Content.ReadFromJsonAsync<ExceptionResponse>();

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            exceptionResponse.Should().NotBeNull();
            exceptionResponse.Message.Should().Be("Not found");
            _clientService.Verify((service) => service.FindByIdAsync(NonExistingId), Times.Once());
        }

        [Fact]
        public async Task FindByIdShouldReturnClientDTOWhenIdExists()
        {
            HttpResponseMessage? response = await _client.GetAsync("/api/clients/" + ExistingId);
            ClientDTO? clientDTO = await response.Content.ReadFromJsonAsync<ClientDTO>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            clientDTO.Should().NotBeNull();
            clientDTO.Id.Should().Be(ClientDTOBuilder.Id);
            _clientService.Verify((service) => service.FindByIdAsync(ExistingId), Times.Once());
        }

        [Fact]
        public async Task SaveShouldReturnBadRequestWhenNameIsNull()
        {
            ClientInsertDTO clientInsertDTO = ClientInsertDTOBuilder.aClientInsertDTO().WithANullName().Build();
            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/clients", clientInsertDTO);
            ValidationProblemDetails validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Name");
            _clientService.Verify((service) => service.SaveAsync(It.IsAny<ClientInsertDTO>()), Times.Never());
        }

        [Fact]
        public async Task SaveShouldReturnBadRequestWhenNameIsEmpty()
        {
            ClientInsertDTO clientInsertDTO = ClientInsertDTOBuilder.aClientInsertDTO().WithAnEmptyName().Build();  
            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/clients", clientInsertDTO);
            ValidationProblemDetails validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Name");
            _clientService.Verify((service) => service.SaveAsync(It.IsAny<ClientInsertDTO>()), Times.Never());
        }

        [Fact]
        public async Task SaveShouldReturnBadRequestWhenNameIsBlank()
        {
            ClientInsertDTO clientInsertDTO = ClientInsertDTOBuilder.aClientInsertDTO().WithAnEmptyName().Build();
            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/clients", clientInsertDTO);
            ValidationProblemDetails validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Name");
            _clientService.Verify((service) => service.SaveAsync(It.IsAny<ClientInsertDTO>()), Times.Never());
        }

        [Fact]
        public async Task SaveShouldReturnBadRequestWhenEmailIsNull()
        {
            ClientInsertDTO clientInsertDTO = ClientInsertDTOBuilder.aClientInsertDTO().WithANullEmail().Build();
            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/clients", clientInsertDTO);
            ValidationProblemDetails validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _clientService.Verify((service) => service.SaveAsync(It.IsAny<ClientInsertDTO>()), Times.Never());
        }

        [Fact]
        public async Task SaveShouldReturnBadRequestWhenEmailIsBlank()
        {
            ClientInsertDTO clientInsertDTO = ClientInsertDTOBuilder.aClientInsertDTO().WithABlankEmail().Build();
            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/clients", clientInsertDTO);
            ValidationProblemDetails validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _clientService.Verify((service) => service.SaveAsync(It.IsAny<ClientInsertDTO>()), Times.Never());
        }

        [Fact]
        public async Task SaveShouldReturnBadRequestWhenEmailIsNotValid()
        {
            ClientInsertDTO clientInsertDTO = ClientInsertDTOBuilder.aClientInsertDTO().WithAnInvalidEmail().Build();

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/clients", clientInsertDTO);
            ValidationProblemDetails validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _clientService.Verify((service) => service.SaveAsync(It.IsAny<ClientInsertDTO>()), Times.Never());
        }

        [Fact]
        public async Task SaveShouldReturnBadRequestWhenEmailIsAlreadyRegistered()
        {
            ClientInsertDTO clientInsertDTO = ClientInsertDTOBuilder.aClientInsertDTO().WithARegisteredEmail().Build();

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/clients", clientInsertDTO);
            ValidationProblemDetails validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _clientService.Verify((service) => service.SaveAsync(It.IsAny<ClientInsertDTO>()), Times.Never());
            _clientRepository.Verify((r) => r.FindByEmail(ClientInsertDTOBuilder.RegisteredEmail), Times.Once());
        }

        [Fact]
        public async Task SaveShouldReturnCreatedWhenDataIsValid()
        {
            ClientInsertDTO clientInsertDTO = ClientInsertDTOBuilder.aClientInsertDTO().Build();

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/clients", clientInsertDTO);
            ClientDTO clientDTO = await response.Content.ReadFromJsonAsync<ClientDTO>();

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            _clientService.Verify((service) => service.SaveAsync(It.IsAny<ClientInsertDTO>()), Times.Once());
            clientDTO.Should().NotBeNull();
            clientDTO.Id.Should().Be(ClientDTOBuilder.Id);
        }

        [Fact]
        public async Task UpdateShouldReturnBadRequestWhenNameIsNull()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().WithANullName().Build();
            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/" + NonExistingId, clientUpdateDTO);
            ValidationProblemDetails validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Name");
            _clientService.Verify((service) => service.UpdateAsync(It.IsAny<ClientUpdateDTO>(), It.IsAny<int>()), Times.Never());
        }

        [Fact]
        public async Task UpdateShouldReturnBadRequestWhenNameIsEmpty()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().WithAnEmptyName().Build();
            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/" + NonExistingId, clientUpdateDTO);
            ValidationProblemDetails validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Name");
            _clientService.Verify((service) => service.UpdateAsync(It.IsAny<ClientUpdateDTO>(), It.IsAny<int>()), Times.Never());
        }

        [Fact]
        public async Task UpdateShouldReturnBadRequestWhenNameIsBlank()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().WithABlankName().Build();
            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/" + NonExistingId, clientUpdateDTO);
            ValidationProblemDetails validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Name");
            _clientService.Verify((service) => service.UpdateAsync(It.IsAny<ClientUpdateDTO>(), It.IsAny<int>()), Times.Never());
        }

        [Fact]
        public async Task UpdateShouldReturnBadRequestWhenEmailIsNull()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().WithANullEmail().Build();
            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/" + NonExistingId, clientUpdateDTO);
            ValidationProblemDetails validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _clientService.Verify((service) => service.UpdateAsync(It.IsAny<ClientUpdateDTO>(), It.IsAny<int>()), Times.Never());
        }

        [Fact]
        public async Task UpdateShouldReturnBadRequestWhenEmailIsBlank()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().WithABlankEmail().Build();
            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/" + NonExistingId, clientUpdateDTO);
            ValidationProblemDetails validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _clientService.Verify((service) => service.UpdateAsync(It.IsAny<ClientUpdateDTO>(), It.IsAny<int>()), Times.Never());
        }

        [Fact]
        public async Task UpdateShouldReturnBadRequestWhenEmailIsInvalid()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().WithAnInvalidEmail().Build();
            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/" + NonExistingId, clientUpdateDTO);
            ValidationProblemDetails validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _clientService.Verify((service) => service.UpdateAsync(It.IsAny<ClientUpdateDTO>(), It.IsAny<int>()), Times.Never());
        }

        [Fact]
        public async Task UpdateShouldReturnBadRequestWhenEmailIsAlreadyRegistered()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().WithARegisteredEmail().Build();
            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/" + ExistingId, clientUpdateDTO);
            ValidationProblemDetails validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _clientService.Verify((service) => service.UpdateAsync(It.IsAny<ClientUpdateDTO>(), It.IsAny<int>()), Times.Never());
            _clientRepository.Verify((r) => r.FindByEmail(ClientUpdateDTOBuilder.RegisteredEmail), Times.Once());
        }

        [Fact]
        public async Task UpdateShouldReturnBadRequestWhenIdIsNotANumber()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().Build();
            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/B", clientUpdateDTO);
            ExceptionResponse? exceptionResponse = await response.Content.ReadFromJsonAsync<ExceptionResponse>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exceptionResponse.Should().NotBeNull();
            exceptionResponse.Message.Should().Be("Id must be a number");
            _clientService.Verify((service) => service.UpdateAsync(It.IsAny<ClientUpdateDTO>(), It.IsAny<int>()), Times.Never());
        }

        [Fact]
        public async Task UpdateShouldReturnNotFoundWhenIdDoesNotExist()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().Build();
            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/" + NonExistingId, clientUpdateDTO);
            ExceptionResponse? exceptionResponse = await response.Content.ReadFromJsonAsync<ExceptionResponse>();

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            exceptionResponse.Should().NotBeNull();
            exceptionResponse.Message.Should().Be("Not found for update");
            _clientService.Verify((s) => s.UpdateAsync(It.IsAny<ClientUpdateDTO>(), NonExistingId), Times.Once());
        }

        [Fact]
        public async Task UpdateShouldReturnSuccessWhenDataIsValidAndIdExists()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().Build();
            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/" + ExistingId, clientUpdateDTO);
            ClientDTO? clientDTO = await response.Content.ReadFromJsonAsync<ClientDTO>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            _clientService.Verify((s) => s.UpdateAsync(It.IsAny<ClientUpdateDTO>(), ExistingId), Times.Once());
            clientDTO.Should().NotBeNull();
            clientDTO.Id.Should().Be(ClientDTOBuilder.Id);
        }

        [Fact]
        public async Task UpdateShouldReturnSuccessWhenTheUserSendTheSameEmail()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().WithARegisteredEmail().Build();
            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/" + ExistingEmailId, clientUpdateDTO);
            ClientDTO? clientDTO = await response.Content.ReadFromJsonAsync<ClientDTO>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            _clientService.Verify((s) => s.UpdateAsync(It.IsAny<ClientUpdateDTO>(), ExistingEmailId), Times.Once());
            clientDTO.Should().NotBeNull();
            clientDTO.Id.Should().Be(ClientDTOBuilder.Id);
        }

        [Fact]
        public async Task DeleteShouldReturnBadRequestWhenIdIsNotANumber()
        {
            HttpResponseMessage? response = await _client.DeleteAsync("api/clients/C");
            ExceptionResponse? exceptionResponse = await response.Content.ReadFromJsonAsync<ExceptionResponse>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exceptionResponse.Should().NotBeNull();
            exceptionResponse.Message.Should().Be("Id must be a number");
            _clientService.Verify((s) => s.DeleteAsync(It.IsAny<int>()), Times.Never());
        }

        [Fact]
        public async Task DeleteShouldReturnNotFoundWhenIdDoesNotExist()
        {
            HttpResponseMessage? response = await _client.DeleteAsync("/api/clients/" + NonExistingId);
            ExceptionResponse? exceptionResponse = await response.Content.ReadFromJsonAsync<ExceptionResponse>();

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            exceptionResponse.Should().NotBeNull();
            exceptionResponse.Message.Should().Be("Not found for delete");
            _clientService.Verify((s) => s.DeleteAsync(NonExistingId), Times.Once());
        }

        [Fact]
        public async Task DeleteShouldReturnNoContentWhenIdExists()
        {
            HttpResponseMessage? response = await _client.DeleteAsync("/api/clients/" + ExistingId);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            _clientService.Verify((s) => s.DeleteAsync(ExistingId), Times.Once());
        }
    }
}
