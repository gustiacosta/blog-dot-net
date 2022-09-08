using AutoMapper;
using Blog.Application.Interfaces;
using Blog.Application.Models;
using Blog.Domain.Entities;
using Blog.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Web.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Editor")]
    [ApiController]
    [Route("api/[controller]")]
    public class BlogPostEditorController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IDateTime _dateTimeService;
        private readonly ILogger<BlogPostEditorController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEntityGenericRepository _repositoryService;

        public BlogPostEditorController(ILogger<BlogPostEditorController> logger, IConfiguration config,
                                        UserManager<ApplicationUser> userManager, IEntityGenericRepository repositoryService, IMapper mapper, IDateTime dateTimeService)
        {
            _mapper = mapper;
            _logger = logger;
            _config = config;
            _userManager = userManager;
            _dateTimeService = dateTimeService;
            _repositoryService = repositoryService;
        }

        /// <summary>
        /// Get all posts in pending approval status
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RequestResponse>> Get()
        {
            try
            {
                var currentUser = await _userManager.FindByEmailAsync(HttpContext.User.Identity.Name);

                var data = await _repositoryService.GetAsync<BlogPost>(
                            c => c.PublishingStatus == (int)Domain.Enums.PostPublishingStatus.PendingApproval,
                            c => c.OrderByDescending(c => c.CreateDate));

                var posts = _mapper.Map<IEnumerable<BlogPostDto>>(data);
                foreach (var post in posts)
                {
                    post.UserName = $"{currentUser.LastName}, {currentUser.Name}";
                }

                return Ok(new RequestResponse
                {
                    IsSuccess = true,
                    Data = posts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return BadRequest(new RequestResponse
            {
                Message = "Error retrieving blog posts"
            });
        }

        /// <summary>
        /// Get posts by status. Current options: 
        /// PendingApproval = 1,
        /// Published = 2,
        /// Rejected = 3
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{status}", Name = "GetByStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RequestResponse>> GetByStatus(int status)
        {
            try
            {
                var currentUser = await _userManager.FindByEmailAsync(HttpContext.User.Identity.Name);

                var data = await _repositoryService.GetAsync<BlogPost>(
                            c => c.UserId.Equals(currentUser.Id) &&
                            c.PublishingStatus == status,
                            c => c.OrderByDescending(c => c.CreateDate),
                            null, null, inc => inc.BlogPostComments);

                var posts = _mapper.Map<IEnumerable<BlogPostDto>>(data);

                foreach (var post in posts)
                {
                    post.UserName = $"{currentUser.LastName}, {currentUser.Name}";
                }

                return Ok(new RequestResponse
                {
                    IsSuccess = true,
                    Data = posts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return BadRequest(new RequestResponse
            {
                Message = "Error retrieving blog posts"
            });
        }

        /// <summary>
        /// Updates a post to: approved/rejected status.
        /// Options to send: 
        /// Published = 2,
        /// Rejected = 3
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>        
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RequestResponse>> Update([FromBody] BlogPostEditorRequest model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var blogPost = await _repositoryService.GetFirstAsync<BlogPost>(c => c.Id == model.BlogPostId);

                    if (blogPost == null)
                    {
                        return NotFound(new RequestResponse
                        {
                            Message = $"Blog post with id: {model.BlogPostId} not found"
                        });
                    }

                    if (blogPost.PublishingStatus == (int)Domain.Enums.PostPublishingStatus.Published)
                    {
                        return BadRequest(new RequestResponse
                        {
                            Message = $"Blog post with ID: {blogPost.Id} is already published"
                        });
                    }

                    if (!model.SetApproved && string.IsNullOrEmpty(model.RejectComment))
                    {
                        return BadRequest(new RequestResponse()
                        {
                            Errors = new List<string>()
                            {
                                "Reject Comment is required"
                            }
                        });
                    }

                    blogPost.PublishingStatus = model.SetApproved ? (int)Domain.Enums.PostPublishingStatus.Published : (int)Domain.Enums.PostPublishingStatus.Rejected;
                    blogPost.PublishDate = model.SetApproved ? _dateTimeService.UtcNow() : null;
                    blogPost.RejectComment = !model.SetApproved ? model.RejectComment : null;
                    blogPost.UpdateDate = _dateTimeService.UtcNow();

                    _repositoryService.Update(blogPost);

                    await _repositoryService.SaveAsync();

                    return Ok(new RequestResponse
                    {
                        IsSuccess = true,
                        Message = $"Blog post with ID: {blogPost.Id} was updated succesfully"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }

                return BadRequest(new RequestResponse
                {
                    Message = "Error updating post"
                });
            }

            return BadRequest(new RequestResponse()
            {
                Errors = new List<string>()
                {
                    "Invalid payload"
                }
            });
        }
    }
}
