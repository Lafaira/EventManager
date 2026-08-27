using EventManager.Application.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Interfaces
{
    public interface IAuthService
    {
        public Task Register(RegisterDto dto);
        public Task<bool> IsHashPasswordEqual(LoginDto dto);
        public Task<string> Login(LoginDto dto);
    }
}
