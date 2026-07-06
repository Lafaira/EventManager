namespace EventManager.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
        public int EventId { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } 
        public DateTime? ProcessedAt { get; set; }
        internal Event? Event { get; private set; }

        public Booking()
        {

        }
        public Booking(int eventId, BookingStatus status)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            Status = status;
            CreatedAt = DateTime.UtcNow;
        }
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Rejected
    }
}
