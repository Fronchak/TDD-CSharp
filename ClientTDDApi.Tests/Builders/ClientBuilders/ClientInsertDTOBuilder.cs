using ClientTDDApi.DTOs.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests.Builders.ClientBuilders
{
    public class ClientInsertDTOBuilder
    {
        public static readonly string Name = "José";
        public static readonly string Email = "jose@gmail.com";
        public static readonly string RegisteredEmail = "carlos@gmail.com";

        private ClientInsertDTO _client;

        private ClientInsertDTOBuilder()
        {
            _client = new ClientInsertDTO()
            {
                Name = Name,
                Email = Email
            };
        }

        public static ClientInsertDTOBuilder aClientInsertDTO()
        {
            return new ClientInsertDTOBuilder();
        }

        public ClientInsertDTOBuilder WithANullName()
        {
            _client.Name = null;
            return this;
        }

        public ClientInsertDTOBuilder WithAnEmptyName()
        {
            _client.Name = "";
            return this;
        }

        public ClientInsertDTOBuilder WithABlankName()
        {
            _client.Name = "  ";
            return this;
        }

        public ClientInsertDTOBuilder WithANullEmail()
        {
            _client.Email = null;
            return this;
        }

        public ClientInsertDTOBuilder WithABlankEmail()
        {
            _client.Email = "  ";
            return this;
        }

        public ClientInsertDTOBuilder WithAnInvalidEmail()
        {
            _client.Email = "mail@gmail";
            return this;
        }
        
        public ClientInsertDTOBuilder WithARegisteredEmail()
        {
            _client.Email = RegisteredEmail;
            return this;
        }

        public ClientInsertDTOBuilder WithEmail(string email)
        {
            _client.Email = email;
            return this;
        }

        public ClientInsertDTO Build()
        {
            return _client;
        }
    }
}
