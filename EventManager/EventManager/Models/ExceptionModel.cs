namespace EventManager.Models
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class NoAvailableSeatsException : Exception
    {
        public NoAvailableSeatsException(string message) : base("No available seats for this event") { }
    }
}
