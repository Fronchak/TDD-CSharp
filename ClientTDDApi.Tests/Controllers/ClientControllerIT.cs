using ClientTDDApi.Data;
using ClientTDDApi.DTOs.Client;
using ClientTDDApi.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data.Common;
using ClientTDDApi.Entities;
using ClientTDDApi.Tests.Builders.ClientBuilders;
using ClientTDDApi.Exceptions;
using ClientTDDApi.Services;
using Microsoft.AspNetCore.Mvc;
using ClientTDDApi.DTOs.Auth;
using System.Net.Http.Headers;
using ClientTDDApi.Tests.Util;
using Newtonsoft.Json.Linq;

namespace ClientTDDApi.Tests.Controllers
{
    public class ClientControllerIT : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly DataContext _context;

        private static readonly string AUTHORIZATION = "Authorization";
        private static readonly string BEARER = "Bearer ";

        public ClientControllerIT(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder((builder) =>
            {
                builder.ConfigureServices((services) =>
                {
                    var dbContextDescriptor = services.SingleOrDefault((d) => d.ServiceType == typeof(DbContextOptions<DataContext>));
                    services.Remove(dbContextDescriptor);
                    var dbConnectionDescriptor = services.SingleOrDefault((d) => d.ServiceType == typeof(DbConnection));
                    services.Remove(dbConnectionDescriptor);
                    services.AddDbContextPool<DataContext>((options) =>
                    {
                        options.UseInMemoryDatabase("client_controller_it");
                    });
                });
            });
            _client = _factory.CreateClient();
            var scope = _factory.Services.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<DataContext>();
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        private async Task<IEnumerable<Client>> AddTwoClientsToDatabase()
        {
            IEnumerable<Client> clients = ClientBuilder.BuildClients();
            _context.Clients.AddRange(clients);
            await _context.SaveChangesAsync();
            return clients;
        }

        private async Task<Client> AddOneClientToDatabase()
        {
            Client client = ClientBuilder.Create().Get();
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return client;
        }

        private async Task<string> GetUserTokenAsync()
        {
            return await TokenUtil.GetToken(_client, AuthTestsSeed.UserEmail, AuthTestsSeed.UserPassword);
        }

        private async Task<string> GetWorkerTokenAsync()
        {
            return await TokenUtil.GetToken(_client, AuthTestsSeed.WorkerEmail, AuthTestsSeed.WorkerPassword);
        }

        private async Task<string> GetAdminTokenAsync()
        {
            return await TokenUtil.GetToken(_client, AuthTestsSeed.AdminEmail, AuthTestsSeed.AdminPassword);
        }

        [Fact]
        public async Task FindAllShouldReturnUnauthorizedWhenUserIsNotAuthenticated()
        {
            HttpResponseMessage? response = await _client.GetAsync("/api/clients");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task FindAllShouldReturnForbiddenWhenANormalUserIsAuthenticated()
        {
            await AuthTestsSeed.Seed(_context);
            string token = await GetUserTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.GetAsync("/api/clients");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task FindAllShouldReturnSuccessWhenAWorkerIsAuthenticated()
        {
            IEnumerable<Client> clients = await AddTwoClientsToDatabase();
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.GetAsync("/api/clients");
            IEnumerable<ClientDTO>? result = await response.Content.ReadFromJsonAsync<IEnumerable<ClientDTO>>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.Should().HaveCount(clients.ToList().Count());
            result.Should().Contain((c) => c.Id == ClientBuilder.Id);
        }

        [Fact]
        public async Task FindAllShouldReturnSuccessWhenAnAdminIsAuthenticated()
        {
            IEnumerable<Client> clients = await AddTwoClientsToDatabase();
            await AuthTestsSeed.Seed(_context);
            string token = await GetAdminTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.GetAsync("/api/clients");
            IEnumerable<ClientDTO>? result = await response.Content.ReadFromJsonAsync<IEnumerable<ClientDTO>>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.Should().HaveCount(clients.ToList().Count());
            result.Should().Contain((c) => c.Id == ClientBuilder.Id);
        }

        [Fact]
        public async Task FindByIdShouldReturnBadRequestWhenIdIsNotANumber()
        {
            HttpResponseMessage? response = await _client.GetAsync("/api/clients/A");
            ExceptionResponse? exceptionResponse = await response.Content.ReadFromJsonAsync<ExceptionResponse>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exceptionResponse.Should().NotBeNull();
            exceptionResponse.Message.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task FindByIdShouldReturnNotFoundWhenIdDoesNotExist()
        {
            await AddOneClientToDatabase();
            int NonExistingId = ClientBuilder.SecondaryId;

            HttpResponseMessage? response = await _client.GetAsync("/api/clients/" + NonExistingId);
            ExceptionResponse? exceptionResponse = await response.Content.ReadFromJsonAsync<ExceptionResponse>();

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            exceptionResponse.Should().NotBeNull();
            exceptionResponse.Message.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task FindByIdShouldReturnSuccessWhenIdExists()
        {
            Client client = await AddOneClientToDatabase();
            int ExistingId = client.Id;

            HttpResponseMessage? response = await _client.GetAsync("/api/clients/" + ExistingId);
            ClientDTO? clientDTO = await response.Content.ReadFromJsonAsync<ClientDTO>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            clientDTO.Should().NotBeNull();
            clientDTO.Id.Should().Be(ExistingId);
        }

        [Fact]
        public async Task DeleteShouldReturnUnhauthorizedWhenUserIsNotAuthenticated()
        {
            await AddOneClientToDatabase();
            HttpResponseMessage? response = await _client.DeleteAsync("/api/clients/1");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _context.Clients.Count().Should().Be(1);
        }

        [Fact]
        public async Task DeleteShouldReturnForbiddenWhenANormalUserIsAuthenticated()
        {
            await AddOneClientToDatabase();
            await AuthTestsSeed.Seed(_context);
            string token = await GetUserTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);
            HttpResponseMessage? response = await _client.DeleteAsync("/api/clients/1");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            _context.Clients.Count().Should().Be(1);
        }

        [Fact]
        public async Task DeleteShouldReturnForbiddenWhenAWorkerIsAuthenticated()
        {
            await AddOneClientToDatabase();
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);
            HttpResponseMessage? response = await _client.DeleteAsync("/api/clients/1");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            _context.Clients.Count().Should().Be(1);
        }

        [Fact]
        public async Task DeleteShouldReturnBadRequestWhenIdIsNotANumberAndAnAdminIsAuthenticated()
        {
            await AddTwoClientsToDatabase();
            await AuthTestsSeed.Seed(_context);
            string token = await GetAdminTokenAsync();

            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);
            HttpResponseMessage? response = await _client.DeleteAsync("/api/clients/C");
            ExceptionResponse? exceptionResponse = await response.Content.ReadFromJsonAsync<ExceptionResponse>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exceptionResponse.Should().NotBeNull();
            exceptionResponse.Message.Length.Should().BeGreaterThan(0);
            _context.Clients.Count().Should().Be(2);
        }

        [Fact]
        public async Task DeleteShouldReturnNotFoundWhenIdDoesNotExistsAndAnAdminIsAuthenticated()
        {
            Client client = await AddOneClientToDatabase();
            int NonExistingId = client.Id + 1;
            await AuthTestsSeed.Seed(_context);
            string token = await GetAdminTokenAsync();

            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);
            HttpResponseMessage? response = await _client.DeleteAsync("/api/clients/" + NonExistingId);
            ExceptionResponse? exceptionResponse = await response.Content.ReadFromJsonAsync<ExceptionResponse>();

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            exceptionResponse.Should().NotBeNull();
            exceptionResponse.Message.Length.Should().BeGreaterThan(0);
            _context.Clients.Count().Should().Be(1);
        }

        [Fact]
        public async Task DeleteShouldReturnNoContentAndRemoveFromDatabaseWhenIdExistsAndAnAdminIsAuthenticated()
        {
            IEnumerable<Client> clients = await AddTwoClientsToDatabase();
            int ExistingId = ClientBuilder.Id;
            await AuthTestsSeed.Seed(_context);
            string token = await GetAdminTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.DeleteAsync("/api/clients/" + ExistingId);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            _context.Clients.Count().Should().Be(1);
            _context.Clients.Any((c) => c.Id == ExistingId).Should().BeFalse();
        }

        [Fact]
        public async Task SaveShouldReturnUnauthorizedWhenUserIsNotAuthenticated()
        {
            ClientInsertDTO clientInsertDTO = ClientInsertDTOBuilder.aClientInsertDTO().Build();

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/clients", clientInsertDTO);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _context.Clients.Count().Should().Be(0);
        }

        [Fact]
        public async Task SaveShouldReturnForbiddenWhenANormalUserIsAuthenticated()
        {
            ClientInsertDTO clientInsertDTO = ClientInsertDTOBuilder.aClientInsertDTO().Build();
            await AuthTestsSeed.Seed(_context);
            string token = await GetUserTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/clients", clientInsertDTO);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            _context.Clients.Count().Should().Be(0);
        }

        [Fact]
        public async Task SaveShouldReturnBadRequestWhenNameIsNullAndAWorkerIsAuthenticated()
        {
            ClientInsertDTO clientInsertDTO = ClientInsertDTOBuilder.aClientInsertDTO().WithANullName().Build();
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/clients", clientInsertDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Name");
            _context.Clients.Count().Should().Be(0);
        }

        [Fact]
        public async Task SaveShouldReturnBadRequestWhenNameIsEmpty()
        {
            ClientInsertDTO clientInsertDTO = ClientInsertDTOBuilder.aClientInsertDTO().WithAnEmptyName().Build();
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/clients", clientInsertDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Name");
            _context.Clients.Count().Should().Be(0);
        }

        [Fact]
        public async Task SaveShouldReturnBadRequestWhenNameIsBlank()
        {
            ClientInsertDTO clientInsertDTO = ClientInsertDTOBuilder.aClientInsertDTO().WithABlankName().Build();
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/clients", clientInsertDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Name");
            _context.Clients.Count().Should().Be(0);
        }

        [Fact]
        public async Task SaveShouldReturnBadRequestWhenEmailIsNull()
        {
            ClientInsertDTO clientInsertDTO = ClientInsertDTOBuilder.aClientInsertDTO().WithANullEmail().Build();
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/clients", clientInsertDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _context.Clients.Count().Should().Be(0);
        }

        [Fact]
        public async Task SaveShouldReturnBadRequestWhenEmailIsBlank()
        {
            ClientInsertDTO clientInsertDTO = ClientInsertDTOBuilder.aClientInsertDTO().WithABlankEmail().Build();
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/clients", clientInsertDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _context.Clients.Count().Should().Be(0);
        }

        [Fact]
        public async Task SaveShouldReturnBadRequestWhenEmailIsInvalid()
        {
            ClientInsertDTO clientInsertDTO = ClientInsertDTOBuilder.aClientInsertDTO().WithAnInvalidEmail().Build();
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/clients", clientInsertDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _context.Clients.Count().Should().Be(0);
        }

        [Fact]
        public async Task SaveShouldReturnBadRequestWhenEmailIsAlreadyRegistered()
        {
            Client client = await AddOneClientToDatabase();
            string email = client.Email;
            ClientInsertDTO clientInsertDTO = ClientInsertDTOBuilder.aClientInsertDTO().WithEmail(email).Build();
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/clients", clientInsertDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _context.Clients.Count().Should().Be(1);
        }

        [Fact]
        public async Task SaveShouldReturnCreatedWhenAddToDatabaseWhenDataIsValidAndAWorkerIsAuthenticated()
        {
            ClientInsertDTO clientInsertDTO = ClientInsertDTOBuilder.aClientInsertDTO().Build();
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/clients", clientInsertDTO);
            ClientDTO? clientDTO = await response.Content.ReadFromJsonAsync<ClientDTO>();

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            _context.Clients.Count().Should().Be(1);
            _context.Clients.Any((c) => c.Email.Equals(clientInsertDTO.Email)).Should().BeTrue();
            clientDTO.Should().NotBeNull();
            clientDTO.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task SaveShouldReturnCreatedWhenAddToDatabaseWhenDataIsValidAndAnAdminIsAuthenticated()
        {
            ClientInsertDTO clientInsertDTO = ClientInsertDTOBuilder.aClientInsertDTO().Build();
            await AuthTestsSeed.Seed(_context);
            string token = await GetAdminTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/clients", clientInsertDTO);
            ClientDTO? clientDTO = await response.Content.ReadFromJsonAsync<ClientDTO>();

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            _context.Clients.Count().Should().Be(1);
            _context.Clients.Any((c) => c.Email.Equals(clientInsertDTO.Email)).Should().BeTrue();
            clientDTO.Should().NotBeNull();
            clientDTO.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateShouldReturnUnauthorizedWhenUserIsNotAuthenticated()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().Build();

            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/1", clientUpdateDTO);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateShouldReturnForbiddenWhenANormalUserIsAuthenticated()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().Build();
            await AuthTestsSeed.Seed(_context);
            string token = await GetUserTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/1", clientUpdateDTO);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdateShouldReturnBadRequestWhenNameIsNull()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().WithANullName().Build();
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/1", clientUpdateDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Name");
            _context.Clients.Count().Should().Be(0);
        }

        [Fact]
        public async Task UpdateShouldReturnBadRequestWhenNameIsEmpty()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().WithAnEmptyName().Build();
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/1", clientUpdateDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Name");
            _context.Clients.Count().Should().Be(0);
        }

        [Fact]
        public async Task UpdateShouldReturnBadRequestWhenNameIsBlank()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().WithABlankName().Build();
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/1", clientUpdateDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Name");
            _context.Clients.Count().Should().Be(0);
        }

        [Fact]
        public async Task UpdateShouldReturnBadRequestWhenEmailIsNull()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().WithANullEmail().Build();
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/1", clientUpdateDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _context.Clients.Count().Should().Be(0);
        }

        [Fact]
        public async Task UpdateShouldReturnBadRequestWhenEmailIsBlank()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().WithABlankEmail().Build();
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/1", clientUpdateDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _context.Clients.Count().Should().Be(0);
        }

        [Fact]
        public async Task UpdateShouldReturnBadRequestWhenEmailIsInvalid()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().WithAnInvalidEmail().Build();
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/1", clientUpdateDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _context.Clients.Count().Should().Be(0);
        }

        [Fact]
        public async Task UpdateShouldReturnBadRequestWhenEmailIsAlreadyRegistered()
        {
            Client client = await AddOneClientToDatabase();
            string email = client.Email;
            int otherId = client.Id + 1;
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().WithEmail(email).Build();
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/" + otherId, clientUpdateDTO);
            ValidationProblemDetails? validationResponse = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            validationResponse.Should().NotBeNull();
            validationResponse.Errors.Keys.Should().Contain("Email");
            _context.Clients.Count().Should().Be(1);
        }

        [Fact]
        public async Task UpdateShoulReturnBadRequestWhenIdIsNotANumber()
        {
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().Build();
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/A", clientUpdateDTO);
            ExceptionResponse? exceptionResponse = await response.Content.ReadFromJsonAsync<ExceptionResponse>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exceptionResponse.Should().NotBeNull();
            exceptionResponse.Message.Length.Should().BeGreaterThan(0);
            _context.Clients.Count().Should().Be(0);
        }

        [Fact]
        public async Task UpdateShouldReturnBadRequestWhenIdDoesNotExist()
        {
            Client client = await AddOneClientToDatabase();
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().Build();
            int NonExistingId = client.Id + 1;
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/" + NonExistingId, clientUpdateDTO);
            ExceptionResponse? exceptionResponse = await response.Content.ReadFromJsonAsync<ExceptionResponse>();

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            exceptionResponse.Should().NotBeNull();
            exceptionResponse.Message.Length.Should().BeGreaterThan(0);
            _context.Clients.Any((c) => c.Email.Equals(clientUpdateDTO.Email)).Should().BeFalse();
        }

        [Fact]
        public async Task UpdateShouldUpdataDatabaseWhenDataIsValidAndIdExistsAndAWorkerIsAuthenticated()
        {
            Client client = await AddOneClientToDatabase();
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().Build();
            int ExistingId = client.Id;
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/" + ExistingId, clientUpdateDTO);
            ClientDTO? clientDTO = await response.Content.ReadFromJsonAsync<ClientDTO>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            _context.Clients.Count().Should().Be(1);
            _context.Clients.Any((c) => c.Email.Equals(clientUpdateDTO.Email)).Should().BeTrue();
            clientDTO.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateShouldUpdataDatabaseWhenDataIsValidAndIdExistsAndAnAdminIsAuthenticated()
        {
            Client client = await AddOneClientToDatabase();
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().Build();
            int ExistingId = client.Id;
            await AuthTestsSeed.Seed(_context);
            string token = await GetAdminTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/" + ExistingId, clientUpdateDTO);
            ClientDTO? clientDTO = await response.Content.ReadFromJsonAsync<ClientDTO>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            _context.Clients.Count().Should().Be(1);
            _context.Clients.Any((c) => c.Email.Equals(clientUpdateDTO.Email)).Should().BeTrue();
            clientDTO.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateShouldNoThrowErrorWhenClientKeepsTheSameEmail()
        {
            Client client = await AddOneClientToDatabase();
            string email = client.Email;
            ClientUpdateDTO clientUpdateDTO = ClientUpdateDTOBuilder.aClientUpdateDTO().WithEmail(email).Build();
            int ExistingId = client.Id;
            await AuthTestsSeed.Seed(_context);
            string token = await GetWorkerTokenAsync();
            _client.DefaultRequestHeaders.Add(AUTHORIZATION, BEARER + token);

            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/clients/" + ExistingId, clientUpdateDTO);
            ClientDTO? clientDTO = await response.Content.ReadFromJsonAsync<ClientDTO>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            _context.Clients.Count().Should().Be(1);
            _context.Clients.Any((c) => c.Email.Equals(clientUpdateDTO.Email)).Should().BeTrue();
            clientDTO.Should().NotBeNull();
        }
    }
}
