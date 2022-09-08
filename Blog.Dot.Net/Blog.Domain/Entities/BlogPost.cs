using System;
using System.Collections.Generic;

namespace Blog.Domain.Entities
{
    public class BlogPost
    {
        public BlogPost()
        {
            BlogPostComments = new HashSet<BlogPostComment>();
            PublishingStatus = (int)Enums.PostPublishingStatus.PendingApproval;
        }
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? PublishDate { get; set; }
        public int PublishingStatus { get; set; }
        public string RejectComment { get; set; }

        public virtual User User { get; set; }
        public virtual IEnumerable<BlogPostComment> BlogPostComments { get; set; }
    }
}
