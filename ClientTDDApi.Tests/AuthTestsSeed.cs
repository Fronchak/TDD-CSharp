using ClientTDDApi.Data;
using ClientTDDApi.Entities;
using ClientTDDApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests
{
    public class AuthTestsSeed
    {
        public static readonly string AdminEmail = "admin@gmail.com";
        public static readonly string AdminPassword = "admin";

        public static readonly string WorkerEmail = "worker@gmail.com";
        public static readonly string WorkerPassword = "worker";

        public static readonly string UserEmail = "user@gmail.com";
        public static readonly string UserPassword = "user";

        public static async Task Seed(DataContext context)
        {
            BCryptPasswordEncoder bCryptPasswordEncoder = new BCryptPasswordEncoder();
            string salt = bCryptPasswordEncoder.GenerateSalt();
            User admin = new User()
            {
                Email = AdminEmail,
                Password = bCryptPasswordEncoder.HashPassword(AdminPassword, salt)
            };
            User worker = new User()
            {
                Email = WorkerEmail,
                Password = bCryptPasswordEncoder.HashPassword(WorkerPassword, salt)
            };
            User user = new User()
            {
                Email = UserEmail,
                Password = bCryptPasswordEncoder.HashPassword(UserPassword, salt)
            };
            Role adminRole = new Role() { Name = "admin" };
            Role workerRole = new Role() { Name = "worker" };
            IEnumerable<UserRole> userRoles = new List<UserRole>()
            {
                new UserRole()
                {
                    User = admin,
                    Role = adminRole
                },
                new UserRole()
                {
                    User = worker,
                    Role = workerRole
                }
            };
            context.UserRoles.AddRange(userRoles);
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
    }
}
