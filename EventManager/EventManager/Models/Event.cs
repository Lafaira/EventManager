using System.ComponentModel.DataAnnotations;

namespace EventManager.Models
{
    public class Event
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        private int _totalSeats;
        public int TotalSeats {
            get => _totalSeats;
            set
            {
                AvailableSeats = value;
                _totalSeats = value;
            }
        }
        public int AvailableSeats { get; set; }
        internal ICollection<Booking> Bookings { get; private set; } = [];
        public Event() { Title = null!; }
        public Event(int id, string title, DateTime startAt, DateTime endAt, int totalSeats, string? description = null, string? location = null)
        {
            Id = id;
            Title = title;
            StartAt = startAt;
            EndAt = endAt;
            TotalSeats = totalSeats;
            AvailableSeats = totalSeats;
            Description = description;
        }

        public bool TryReserveSeats(int count = 1)
        {
            var seats = AvailableSeats - count;

            if (AvailableSeats == 0 || seats < 0)
                return false;

            AvailableSeats = seats;
            return true;
        }

        public void ReleaseSeats(int count = 1)
        {
            AvailableSeats += count;
        }
    }
}
