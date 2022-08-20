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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Writer")]
    [ApiController]
    [Route("api/[controller]")]
    public class BlogPostWriterController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IDateTime _dateTimeService;
        private readonly ILogger<BlogPostWriterController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEntityGenericRepository _repositoryService;

        public BlogPostWriterController(ILogger<BlogPostWriterController> logger, IConfiguration config,
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
        /// Get all posts created by the logged in writer
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RequestResponse>> Get()
        {
            try
            {
                var currentUser = await _userManager.FindByEmailAsync(HttpContext.User.Identity.Name);

                var data = await _repositoryService.GetAsync<BlogPost>(
                            c => c.UserId.Equals(currentUser.Id),
                            c => c.OrderByDescending(c => c.CreateDate),
                            null, null, inc => inc.BlogPostComments);

                var posts = _mapper.Map<IEnumerable<BlogPostDto>>(data);

                return Ok(new RequestResponse
                {
                    IsSuccess = true,
                    Data = _mapper.Map<IEnumerable<BlogPostDto>>(data)
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
        /// Creates a new post (only users with 'Writer' role) with initial state 'Pending Approval'        
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //[Authorize(Roles = "Writer")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RequestResponse>> Add([FromBody] BlogPostAddNewRequest model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _userManager.FindByEmailAsync(HttpContext.User.Identity.Name); //User.Identity.Name);

                    var newPost = new BlogPost
                    {
                        Title = model.Title,
                        Content = model.Content,
                        UserId = currentUser.Id,
                        CreateDate = _dateTimeService.UtcNow()
                    };

                    // let's validate existing posts for this user with same partial info
                    var checkExistingPost = await _repositoryService
                        .GetFirstAsync<BlogPost>(c => c.UserId.Equals(newPost.UserId) && c.Title.Equals(model.Title));

                    if (checkExistingPost != null)
                    {
                        return BadRequest(new RequestResponse
                        {
                            Message = $"You already have a post with the title: {model.Title}"
                        });
                    }

                    _repositoryService.Create(newPost);

                    await _repositoryService.SaveAsync();

                    //return CreatedAtAction(nameof(Add), new
                    //{
                    //    Id = newPost.Id,
                    //    IsSuccess = true
                    //}, newPost);

                    return Ok(new RequestResponse
                    {
                        IsSuccess = true,
                        Message = $"Your post with ID: {newPost.Id} was created succesfully and will be reviewed by an Editor"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }

                return BadRequest(new RequestResponse
                {
                    Message = "Error submiting new post"
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

        /// <summary>
        /// Updates a blog post.
        /// Posts can not be edited or updated while in pending status, only when rejected.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RequestResponse>> Update([FromBody] BlogPostUpdateRequest model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _userManager.FindByEmailAsync(HttpContext.User.Identity.Name);

                    var blogPost = await _repositoryService.GetFirstAsync<BlogPost>(c => c.Id == model.BlogPostId);

                    if (blogPost == null)
                    {
                        return NotFound(new RequestResponse
                        {
                            Message = $"Blog post with id: {model.BlogPostId} not found"
                        });
                    }

                    if (!blogPost.UserId.Equals(currentUser.Id))
                    {
                        return BadRequest(new RequestResponse
                        {
                            Message = $"Blog post with ID: {blogPost.Id} is not your post"
                        });
                    }

                    if (blogPost.PublishingStatus == (int)Domain.Enums.PostPublishingStatus.Published)
                    {
                        return BadRequest(new RequestResponse
                        {
                            Message = $"Blog post with ID: {blogPost.Id} is already published. Can not be modified"
                        });
                    }

                    if (blogPost.PublishingStatus == (int)Domain.Enums.PostPublishingStatus.PendingApproval)
                    {
                        return BadRequest(new RequestResponse
                        {
                            Message = $"Blog post with ID: {blogPost.Id} is in Pending Approval. Can not be modified"
                        });
                    }

                    blogPost.Title = model.Title;
                    blogPost.Content = model.Content;
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
