using System;
using System.Collections.Generic;

namespace Blog.Application.Models
{
    public class BlogPostDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? PublishDate { get; set; }
        public int PublishingStatus { get; set; }
        public IEnumerable<BlogPostCommentDto> Comments { get; set; }
    }
}
