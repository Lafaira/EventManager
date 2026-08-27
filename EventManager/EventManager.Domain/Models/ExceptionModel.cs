namespace EventManager.DomainModels
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class NoAvailableSeatsException : Exception
    {
        public NoAvailableSeatsException(string message) : base("No available seats for this event") { }
    }

    public class EventHasEndedException : Exception
    {
        public EventHasEndedException(string message) : base("Cannot book, event completed") { }
    }

    public class BookingLimitExceededException : Exception
    {
        public BookingLimitExceededException(string message) : base("Booking limit exceeded") { }
    }
    public class NoRightsException : Exception
    {
        public NoRightsException(string message) : base("No rights to perform the operation") { }
    }

}
