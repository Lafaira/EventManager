using EventManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Dto
{
    public class RegisterDto
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public RolesEnum Roles { get; set; } = RolesEnum.User;
    }
}
