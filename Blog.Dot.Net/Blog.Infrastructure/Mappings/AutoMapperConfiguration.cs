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
                .ForMember(dest => dest.Comments, member => member.MapFrom(source => source.BlogPostComments != null ? source.BlogPostComments : null))
                .ForMember(dest => dest.CommentsCount, member => member.MapFrom(dest => dest.BlogPostComments != null ? dest.BlogPostComments.Count() : 0))
                .ForMember(dest => dest.IsPublished, member => member.MapFrom(source => source.PublishingStatus == (int)Domain.Enums.PostPublishingStatus.Published))
                .ForMember(dest => dest.UserName, member => member.MapFrom(source => source.User != null ? $"{source.User.Name} {source.User.LastName}" : string.Empty));

            CreateMap<BlogPostComment, BlogPostCommentDto>()
                .ForMember(dest => dest.UserName, member => member.MapFrom(source => source.User != null ? $"{source.User.Name} {source.User.LastName}" : string.Empty)); //.ReverseMap();
        }
    }
}
