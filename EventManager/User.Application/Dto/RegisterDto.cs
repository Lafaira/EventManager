using Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using UserService.User.Domain.Models;

namespace UserService.User.Application.Dto
{
    public class RegisterDto
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public RolesEnum Roles { get; set; } = RolesEnum.User;
    }
}
