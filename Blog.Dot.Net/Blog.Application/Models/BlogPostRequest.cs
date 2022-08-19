using System.ComponentModel.DataAnnotations;

namespace Blog.Application.Models
{
    /// <summary>
    /// Used for adding or updating posts
    /// </summary>
    public class BlogPostRequest
    {
        public int BlogPostId { get; set; } //required only when updating

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
