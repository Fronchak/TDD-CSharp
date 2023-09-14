using ClientTDDApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests.Builders.Users
{
    public class UserBuilder
    {
        public static readonly int Id = 1;
        public static readonly string Email = "matheus@gmail.com";
        public static readonly string Password = "hashed1";

        public static readonly int SecondaryId = 2;
        public static readonly string SecondaryEmail = "ana@gmail.com";
        public static readonly string SecondaryPassword = "hashed2";

        public static readonly IEnumerable<Role> Roles = new List<Role>()
        {
            new Role()
            {
                Id = 1,
                Name = "Role 1",
            },
            new Role()
            {
                Id = 2,
                Name = "Role 2"
            }
        };

        public static readonly IEnumerable<Role> SecondaryRoles = new List<Role>()
        {
            new Role()
            {
                Id = 3,
                Name = "Role 3"
            },
            new Role()
            {
                Id = 4,
                Name = "Role 4"
            }
        };

        private User _user;

        private UserBuilder()
        {
            _user = new User()
            {
                Id = Id,
                Email = Email,
                Password = Password
            };
            IEnumerable<UserRole> userRoles = Roles.ToList()
                .Select((r) => new UserRole()
                {
                    User = _user,
                    Role = r
                });
            _user.UserRoles = userRoles;
        }

        public static UserBuilder AUser()
        {
            return new UserBuilder();
        }

        public UserBuilder WithSecondaryValues()
        {
            _user = new User()
            {
                Id = SecondaryId,
                Email = SecondaryEmail,
                Password = SecondaryPassword
            };
            IEnumerable<UserRole> userRoles = SecondaryRoles.ToList()
                .Select((r) => new UserRole()
                {
                    User = _user,
                    Role = r
                });
            _user.UserRoles = userRoles;
            return this;
        }

        public User Build()
        {
            return _user;
        }

        public static IEnumerable<User> BuildUsers()
        {
            User user1 = AUser().Build();
            User user2 = AUser().WithSecondaryValues().Build();
            return new[] { user1, user2 };
        }
    }
}
