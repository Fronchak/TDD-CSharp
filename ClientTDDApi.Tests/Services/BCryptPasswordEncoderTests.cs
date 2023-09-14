using ClientTDDApi.Services;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTDDApi.Tests.Services
{
    public class BCryptPasswordEncoderTests
    {
        [Fact]
        public void GenerateSaltShouldReturnAString()
        {
            BCryptPasswordEncoder passwordEncoder = new BCryptPasswordEncoder();

            string result = passwordEncoder.GenerateSalt();

            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public void HashPasswordShouldReturnADifferentPassword()
        {
            BCryptPasswordEncoder passwordEncoder = new BCryptPasswordEncoder();
            string password = "123456";
            string salt = passwordEncoder.GenerateSalt();

            string result = passwordEncoder.HashPassword(password, salt);

            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
            result.Should().NotBe(password);
        }

        [Fact]
        public void VerifyPasswordShouldReturnFalseWhenPasswordIsIncorrect()
        {
            BCryptPasswordEncoder passwordEncoder = new BCryptPasswordEncoder();
            string password = "123456";
            string salt = passwordEncoder.GenerateSalt();
            string hashed = passwordEncoder.HashPassword(password, salt);

            bool result = passwordEncoder.VerifyPassword("12345", hashed);

            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyPasswordShouldReturnTrueWhenPasswordIsCorrect()
        {
            BCryptPasswordEncoder passwordEncoder = new BCryptPasswordEncoder();
            string password = "123456";
            string salt = passwordEncoder.GenerateSalt();
            string hashed = passwordEncoder.HashPassword(password, salt);

            bool result = passwordEncoder.VerifyPassword("123456", hashed);

            result.Should().BeTrue();
        }
    }
}
