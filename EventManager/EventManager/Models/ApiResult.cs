using System.Net;

namespace EventManager.Models
{
    public class ApiBaseResult
    {
        public required bool Success { get; set; }
        public required HttpStatusCode StatusCode { get; set; }
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        public required string Message { get; set; }
    }

    public class ApiResult<T> : ApiBaseResult
    {
        public required T Data { get; set; }
    }

    public class ApiResult : ApiBaseResult
    {
        
    }

}
