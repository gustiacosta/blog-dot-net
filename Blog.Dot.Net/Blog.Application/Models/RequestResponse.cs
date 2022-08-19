namespace Blog.Application.Models
{
    public class RequestResponse
    {
        public bool IsSuccess { get; set; } = false;
        public object Data { get; set; } = null;
        public object Errors { get; set; } = null;
        public string Message { get; set; } = string.Empty;
    }
}
