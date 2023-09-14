using ClientTDDApi.DTOs.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests.Builders.ClientBuilders
{
    public class ClientUpdateDTOBuilder
    {
        public static readonly string Name = "José";
        public static readonly string Email = "jose@gmail.com";
        public static readonly string RegisteredEmail = "carlos@gmail.com";

        private ClientUpdateDTO _client;

        private ClientUpdateDTOBuilder()
        {
            _client = new ClientUpdateDTO()
            {
                Name = Name,
                Email = Email
            };
        }

        public static ClientUpdateDTOBuilder aClientUpdateDTO()
        {
            return new ClientUpdateDTOBuilder();
        }

        public ClientUpdateDTOBuilder WithANullName()
        {
            _client.Name = null;
            return this;
        }

        public ClientUpdateDTOBuilder WithAnEmptyName()
        {
            _client.Name = "";
            return this;
        }

        public ClientUpdateDTOBuilder WithABlankName()
        {
            _client.Name = "  ";
            return this;
        }

        public ClientUpdateDTOBuilder WithANullEmail()
        {
            _client.Email = null;
            return this;
        }

        public ClientUpdateDTOBuilder WithABlankEmail()
        {
            _client.Email = "  ";
            return this;
        }

        public ClientUpdateDTOBuilder WithAnInvalidEmail()
        {
            _client.Email = "mail@gmail";
            return this;
        }

        public ClientUpdateDTOBuilder WithARegisteredEmail()
        {
            _client.Email = RegisteredEmail;
            return this;
        }

        public ClientUpdateDTOBuilder WithEmail(string email)
        {
            _client.Email = email;
            return this;
        }

        public ClientUpdateDTO Build()
        {
            return _client;
        }
    }
}
