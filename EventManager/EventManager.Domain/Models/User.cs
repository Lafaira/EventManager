using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Domain.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Login {  get; set; }
        public string HashPassword { get; set; }
        public RolesEnum Roles { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        public User()
        {
            Id = Guid.NewGuid();
        }
    }
}
