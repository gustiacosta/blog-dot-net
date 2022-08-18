using AutoMapper;
using Blog.Application.Models;
using Blog.Domain.Entities;

namespace Blog.Application.Mappings
{
    public class AutoMapperConfiguration : Profile
    {
        public AutoMapperConfiguration()
        {
            CreateMap<BlogPost, BlogPostDto>()
                .ForMember(dest => dest.Comments, member => member.MapFrom(source => source.BlogPostComments));

            CreateMap<BlogPostComment, BlogPostCommentDto>(); //.ReverseMap();
        }
    }
}
