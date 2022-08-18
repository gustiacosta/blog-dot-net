using System;

namespace Blog.Domain.Entities
{
    public class BlogPostComment
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public int BlogPostId { get; set; }
        public DateTime Date { get; set; }
        public string Comment { get; set; }
        public virtual BlogPost BlogPost { get; set; }
    }
}
