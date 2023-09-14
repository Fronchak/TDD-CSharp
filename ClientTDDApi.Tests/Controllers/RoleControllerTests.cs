using ClientTDDApi.DTOs.Role;
using ClientTDDApi.Interfaces;
using ClientTDDApi.Tests.Builders.RoleBuilders;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Json;

namespace ClientTDDApi.Tests.Controllers
{
    public class RoleControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly Mock<IRoleService> _roleService;

        public RoleControllerTests(WebApplicationFactory<Program> factory)
        {
            IEnumerable<RoleDTO> roleDTOs = RoleDTOBuilder.BuildRoleDTOs();
            _roleService = new Mock<IRoleService>();
            _roleService.Setup((s) => s.FindAllAsync()).ReturnsAsync(roleDTOs);
            _factory = factory.WithWebHostBuilder((builder) =>
            {
                builder.ConfigureServices((services) =>
                {
                    services.AddScoped((sp) => _roleService.Object);
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task FindAllShouldReturnRoleDTOsList()
        {
            HttpResponseMessage? response = await _client.GetAsync("/api/roles");
            IEnumerable<RoleDTO>? content = await response.Content.ReadFromJsonAsync<IEnumerable<RoleDTO>>();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().NotBeNull().And.HaveCount(2);
            _roleService.Verify((s) => s.FindAllAsync(), Times.Once());
        }

    }
}
