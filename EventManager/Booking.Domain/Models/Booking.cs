namespace BookingService.Booking.Domain.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
        public int EventId { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } 
        public DateTime? ProcessedAt { get; set; }
        public Guid UserId { get; set; }

        public Booking()
        {

        }
        public Booking(int eventId, BookingStatus status, Guid userId)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            Status = status;
            CreatedAt = DateTime.UtcNow;
            UserId = userId;
        }

        public void Cancelled()
        {
            if (Status != BookingStatus.Cancelled)
                Status = BookingStatus.Cancelled;
            else
                throw new Exception("");
        }
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Rejected,
        Cancelled
    }
}
