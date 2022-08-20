using System.ComponentModel.DataAnnotations;

namespace Blog.Application.Models
{
    /// <summary>
    /// Used for adding posts
    /// </summary>
    public class BlogPostAddNewRequest
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
