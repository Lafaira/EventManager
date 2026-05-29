namespace EventManager.Models.RequestModel
{
    public class GetEventsQuery
    {
        public string? Title { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
