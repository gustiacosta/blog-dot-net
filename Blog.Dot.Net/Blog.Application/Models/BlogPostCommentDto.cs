using System;

namespace Blog.Application.Models
{
    public class BlogPostCommentDto
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int BlogPostId { get; set; }
        public DateTime Date { get; set; }
        public string Comment { get; set; }
    }
}
