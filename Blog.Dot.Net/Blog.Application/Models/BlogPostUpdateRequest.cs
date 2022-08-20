using System.ComponentModel.DataAnnotations;

namespace Blog.Application.Models
{
    /// <summary>
    /// Used for updating posts
    /// </summary>
    public class BlogPostUpdateRequest
    {
        [Required]
        public int BlogPostId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
