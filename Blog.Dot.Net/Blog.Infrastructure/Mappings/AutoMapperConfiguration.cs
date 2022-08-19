using AutoMapper;
using Blog.Application.Models;
using Blog.Domain.Entities;
using System.Linq;

namespace Blog.Application.Mappings
{
    public class AutoMapperConfiguration : Profile
    {
        public AutoMapperConfiguration()
        {
            CreateMap<BlogPost, BlogPostDto>()
                .ForMember(dest => dest.IsPublished, member => member.MapFrom(source => source.PublishingStatus == (int)Domain.Enums.PostPublishingStatus.Published))
                .ForMember(dest => dest.Comments, member => member.MapFrom(source => source.BlogPostComments))
                .ForMember(dest => dest.CommentsCount, member => member.MapFrom(dest => dest.BlogPostComments != null ? dest.BlogPostComments.Count() : 0));

            CreateMap<BlogPostComment, BlogPostCommentDto>(); //.ReverseMap();
        }
    }
}
