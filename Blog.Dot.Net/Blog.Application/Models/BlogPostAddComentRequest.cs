using System.ComponentModel.DataAnnotations;

namespace Blog.Application.Models
{
    public class BlogPostAddCommentRequest
    {
        [Required]
        public int BlogPostId { get; set; }

        [Required]
        public string Comment { get; set; }
    }
}
