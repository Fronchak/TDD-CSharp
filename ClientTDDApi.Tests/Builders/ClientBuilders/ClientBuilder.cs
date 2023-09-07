using ClientTDDApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests.Builders.ClientBuilders
{
    public class ClientBuilder
    {
        private Client Client;

        private ClientBuilder()
        {
            Client = new Client();
        }

        public static ClientBuilder Create()
        {
            return new ClientBuilder();
        }

        public ClientBuilder WithId(int id)
        {
            Client.Id = id;
            return this;
        }

        public ClientBuilder WithName(string name)
        {
            Client.Name = name;
            return this;
        }

        public ClientBuilder WithEmail(string email)
        {
            Client.Email = email;
            return this;
        }

        public Client Get()
        {
            return Client;
        }
    }
}
