namespace EventManager.Models
{
    public class Booking
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int EventId { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ProcessedAt { get; set; }
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Rejected
    }
}
