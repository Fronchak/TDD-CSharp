using ClientTDDApi.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests.Builders.Auth
{
    public class UserRegisterDTOBuilder
    {
        public static readonly string Email = "mail@gmail.com";
        public static readonly string InvalidEmail = "mail@gmail";
        public static readonly string RegisteredEmail = "register@gmail.com";
        public static readonly string Password = "password";
        public static readonly string DiffPassword = "other password";

        private UserRegisterDTO _dto;

        private UserRegisterDTOBuilder()
        {
            _dto = new UserRegisterDTO()
            {
                Email = Email,
                Password = Password,
                ConfirmPassword = Password
            };
        }

        public static UserRegisterDTOBuilder AUserRegisterDTO()
        {
            return new UserRegisterDTOBuilder();
        }

        public UserRegisterDTOBuilder WithANullEmail()
        {
            _dto.Email = null;
            return this;
        }

        public UserRegisterDTOBuilder WithABlankEmail()
        {
            _dto.Email = "  ";
            return this;
        }

        public UserRegisterDTOBuilder WithAnInvalidEmail()
        {
            _dto.Email = InvalidEmail;
            return this;
        }

        public UserRegisterDTOBuilder WithARegisterEmail()
        {
            _dto.Email = RegisteredEmail;
            return this;
        }

        public UserRegisterDTOBuilder WithEmail(string email)
        {
            _dto.Email = email;
            return this;
        }

        public UserRegisterDTOBuilder WithANullPassword()
        {
            _dto.Password = null;
            _dto.ConfirmPassword = null;
            return this;
        }

        public UserRegisterDTOBuilder WithAShortPassword()
        {
            _dto.Password = "123";
            _dto.ConfirmPassword = "123";
            return this;
        }

        public UserRegisterDTOBuilder WithDifferentPasswords()
        {
            _dto.Password = Password;
            _dto.ConfirmPassword = DiffPassword;
            return this;
        }

        public UserRegisterDTO Build()
        {
            return _dto;
        }
    }
}
