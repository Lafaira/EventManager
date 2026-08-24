using EventManager.Application.Dto;
using EventManager.Application.Interfaces;
using EventManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace EventManager.Application.Services
{
    public class AuthService : IAuthService
    {
        IUserRepository _userRepository;
        IConfiguration _configuration;
        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task Register(RegisterDto dto)
        {
            var user = new User()
            {
                Login = dto.Login,
                HashPassword = GetHashPassword(dto.Password),
                Roles = dto.Roles
            };

            await _userRepository.AddUser(user);

            await _userRepository.SaveChangesAsync();
        }

        public async Task<string> Login(LoginDto dto)
        {
            var saveUser = await _userRepository.GetUser(dto.Login);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, dto.Login),
                new Claim(ClaimTypes.Role, saveUser.Roles.ToString()),
                new Claim(ClaimTypes.NameIdentifier, saveUser.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthData:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            int expMinutes = int.Parse(_configuration["AuthData:exp"]!);

            var token = new JwtSecurityToken(
                issuer: _configuration["AuthData:iss"],
                audience: _configuration["AuthData:aud"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> IsHashPasswordEqual(LoginDto dto)
        {
            var user = await _userRepository.GetUser(dto.Login);

            var haskPassword = GetHashPassword(dto.Password);

            if(user == null || user.HashPassword != haskPassword)
                return false;

            return true;
        }

        private string GetHashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }
    }
}
