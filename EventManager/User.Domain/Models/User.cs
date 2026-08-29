using Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace UserService.User.Domain.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Login {  get; set; }
        public string HashPassword { get; set; }
        public RolesEnum Roles { get; set; }

        public User()
        {
            Id = Guid.NewGuid();
        }
    }
}
