using System;
using System.Collections.Generic;
using System.Text;
using UserService.User.Application.Dto;

namespace UserService.User.Application.Interfaces
{
    public interface IAuthService
    {
        public Task Register(RegisterDto dto);
        public Task<bool> IsHashPasswordEqual(LoginDto dto);
        public Task<string> Login(LoginDto dto);
    }
}
