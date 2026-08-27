using EventManager.Application.Dto;
using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Domain.Models;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventManager.Tests
{
    public class AuthServiceTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly IConfiguration _configuration;
        private readonly AuthService _authService;

        public AuthServiceTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();

            var inMemorySettings = new Dictionary<string, string>
        {
            {"AuthData:SecretKey", "SuperLongSecretKey1234567890123456"},
            {"AuthData:iss", "MyAuthServer"},
            {"AuthData:aud", "MyApi"},
            {"AuthData:exp", "30"}
        };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _authService = new AuthService(_userRepositoryMock.Object, _configuration);
        }

        [Fact]
        public async Task Register_Success()
        {
            var dto = new RegisterDto
            {
                Login = "test",
                Password = "test",
                Roles  = RolesEnum.User
            };

            _userRepositoryMock
                .Setup(x => x.AddUser(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _userRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            await _authService.Register(dto);

            _userRepositoryMock.Verify(x => x.AddUser(
                It.Is<User>(u =>
                    u.Login == dto.Login &&
                    u.Roles == dto.Roles &&
                    !string.IsNullOrEmpty(u.HashPassword))),
                Times.Once);

            _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Register_CreatesCorrectPasswordHash()
        {
            var dto = new RegisterDto
            {
                Login = "test",
                Password = "test",
                Roles = RolesEnum.User
            };

            User? capturedUser = null;

            _userRepositoryMock
                .Setup(x => x.AddUser(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Callback<User, CancellationToken>((user, ct) => capturedUser = user)
                .Returns(Task.CompletedTask);

            _userRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            await _authService.Register(dto);

            Assert.NotNull(capturedUser);
            Assert.NotEmpty(capturedUser.HashPassword);

            var expectedHash = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    Encoding.UTF8.GetBytes("test")));

            Assert.Equal(expectedHash, capturedUser.HashPassword);
        }

        [Fact]
        public async Task Login_ReturnsValidJwtToken()
        {
            var userId = Guid.NewGuid();
            var dto = new LoginDto
            {
                Login = "test",
                Password = "test"
            };

            var user = new User
            {
                Id = userId,
                Login = "test",
                HashPassword = Convert.ToHexString(
                    System.Security.Cryptography.SHA256.HashData(
                        Encoding.UTF8.GetBytes("test"))),
                Roles = RolesEnum.Admin
            };

            _userRepositoryMock
                .Setup(x => x.GetUser(dto.Login))
                .ReturnsAsync(user);

            var token = await _authService.Login(dto);

            Assert.NotNull(token);
            Assert.NotEmpty(token);

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            Assert.Equal("MyAuthServer", jwtToken.Issuer);
            Assert.Contains("MyApi", jwtToken.Audiences);

            Assert.Equal("test", jwtToken.Claims.First(c => c.Type == ClaimTypes.Name).Value);
            Assert.Equal("Admin", jwtToken.Claims.First(c => c.Type == ClaimTypes.Role).Value);
            Assert.Equal(userId.ToString(), jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            Assert.True(jwtToken.ValidTo > DateTime.UtcNow);
            Assert.True(jwtToken.ValidTo <= DateTime.UtcNow.AddMinutes(31));
        }
        

        [Fact]
        public async Task IsHashPasswordEqual_ReturnsTrue_WhenPasswordCorrect()
        {
            var password = "test";
            var hashPassword = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    Encoding.UTF8.GetBytes(password)));

            var dto = new LoginDto
            {
                Login = "test",
                Password = password
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Login = "test",
                HashPassword = hashPassword,
                Roles = RolesEnum.User
            };

            _userRepositoryMock
                .Setup(x => x.GetUser(dto.Login))
                .ReturnsAsync(user);

            var result = await _authService.IsHashPasswordEqual(dto);


            Assert.True(result);
        }

        [Fact]
        public async Task IsHashPasswordEqual_ReturnsFalse_WhenPasswordIncorrect()
        {
            var dto = new LoginDto
            {
                Login = "test",
                Password = "test"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Login = "test",
                HashPassword = Convert.ToHexString(
                    System.Security.Cryptography.SHA256.HashData(
                        Encoding.UTF8.GetBytes("test1"))),
                Roles = RolesEnum.User
            };

            _userRepositoryMock
                .Setup(x => x.GetUser(dto.Login))
                .ReturnsAsync(user);

            var result = await _authService.IsHashPasswordEqual(dto);

            Assert.False(result);
        }

        [Fact]
        public async Task IsHashPasswordEqual_ReturnsFalse_WhenUserNotFound()
        {
            var dto = new LoginDto
            {
                Login = "test",
                Password = "test"
            };

            _userRepositoryMock
                .Setup(x => x.GetUser(dto.Login))
                .ReturnsAsync((User)null);

            var result = await _authService.IsHashPasswordEqual(dto);

            Assert.False(result);
        }

        [Fact]
        public async Task Register_CallsSaveChangesAfterAddUser()
        {
            var dto = new RegisterDto
            {
                Login = "test",
                Password = "test",
                Roles = RolesEnum.User
            };

            var callOrder = new List<string>();

            _userRepositoryMock
                .Setup(x => x.AddUser(It.IsAny<User>()))
                .Callback(() => callOrder.Add("AddUser"))
                .Returns(Task.CompletedTask);

            _userRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Callback(() => callOrder.Add("SaveChanges"))
                .Returns(Task.CompletedTask);

            await _authService.Register(dto);

            Assert.Equal(new[] { "AddUser", "SaveChanges" }, callOrder);
        }

    [Fact]
        public async Task Login_ThrowsException_WhenUserNotFound()
        {
            var dto = new LoginDto
            {
                Login = "test",
                Password = "test"
            };

            _userRepositoryMock
                .Setup(x => x.GetUser(dto.Login))
                .ReturnsAsync((User)null);

            await Assert.ThrowsAsync<NullReferenceException>(
                async () => await _authService.Login(dto));
        }
    }
}
