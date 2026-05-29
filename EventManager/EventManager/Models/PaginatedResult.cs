namespace EventManager.Models
{
    public class PaginatedResult
    {
        public int CountEvent { get; set; }
        public Event[] EventArr { get; set; }
        public int NumberCurrentPage { get; set; }
        public int CountEventInPage { get; set; }
    }
}
