using System.ComponentModel.DataAnnotations;

namespace Blog.Application.Models
{
    public class BlogPostEditorRequest
    {
        [Required]
        public int BlogPostId { get; set; }

        [Required]
        public bool SetApproved { get; set; }

        public string RejectComment { get; set; }
    }
}
