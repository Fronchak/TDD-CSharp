using ClientTDDApi.Data;
using ClientTDDApi.DTOs.Client;
using ClientTDDApi.Entities;
using ClientTDDApi.Tests.Builders.RoleBuilders;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ClientTDDApi.DTOs.Role;

namespace ClientTDDApi.Tests.Controllers
{
    public class RoleControllerIT : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly DataContext _context;

        public RoleControllerIT(WebApplicationFactory<Program> factory)
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
                        options.UseInMemoryDatabase("fake_database");
                    });
                });
            });
            _client = _factory.CreateClient();
            var scope = _factory.Services.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<DataContext>();
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task FindAllShouldReturnRoleDTOList()
        {
            IEnumerable<Role> roles = RoleBuilder.BuildRoles();
            _context.Roles.AddRange(roles); 
            await _context.SaveChangesAsync();

            HttpResponseMessage? response = await _client.GetAsync("/api/roles");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            IEnumerable<RoleDTO>? result = await response.Content.ReadFromJsonAsync<IEnumerable<RoleDTO>>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.Should().HaveCount(roles.Count());
            result.Should().Contain((c) => c.Id == RoleBuilder.Id);
        }
    }
}
