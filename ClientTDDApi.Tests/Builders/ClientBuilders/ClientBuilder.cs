using ClientTDDApi.DTOs.Client;
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
        public static int Id = 10;
        public static string Name = "Ana Clara";
        public static string Email = "ana@gmail.com";

        public static int SecondaryId = 11;
        public static string SecondaryName = "Bruno Carlos";
        public static string SecondaryEmail = "bruno@gmail.com";

        private Client Client;

        private ClientBuilder()
        {
            Client = new Client()
            {
                Id = Id,
                Name = Name,
                Email = Email
            };
        }

        public static ClientBuilder Create()
        {
            return new ClientBuilder();
        }

        public ClientBuilder WithSecondaryValues()
        {
            Client = new Client()
            {
                Id = SecondaryId,
                Name = SecondaryName,
                Email = SecondaryEmail
            };
            return this;
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

        public static IEnumerable<Client> BuildClients()
        {
            Client client1 = Create().Get();
            Client client2 = Create().WithSecondaryValues().Get();
            return new List<Client>() { client1, client2 };
        }
    }
}
