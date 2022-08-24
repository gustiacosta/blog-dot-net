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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Public,Writer,Editor")]
    [ApiController]
    [Route("api/[controller]")]
    public class BlogPostController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IDateTime _dateTimeService;
        private readonly ILogger<BlogPostController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEntityGenericRepository _repositoryService;

        public BlogPostController(ILogger<BlogPostController> logger, IConfiguration config,
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
        /// Get list of all published posts (any role)
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
                var data = await _repositoryService.GetAsync<BlogPost>(
                            c => c.PublishingStatus == (int)Domain.Enums.PostPublishingStatus.Published,
                            c => c.OrderByDescending(c => c.PublishDate),
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
        /// Add a comment to a published post (any role)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("addcomment")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RequestResponse>> AddComment([FromBody] BlogPostAddCommentRequest model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _userManager.FindByEmailAsync(HttpContext.User.Identity.Name); //User.Identity.Name);

                    var blogPost = await _repositoryService.GetFirstAsync<BlogPost>(c => c.Id == model.BlogPostId);

                    if (blogPost == null)
                    {
                        return NotFound(new RequestResponse
                        {
                            Message = $"Blog post with id: {model.BlogPostId} not found"
                        });
                    }

                    if (blogPost.PublishingStatus != (int)Domain.Enums.PostPublishingStatus.Published)
                    {
                        return BadRequest(new RequestResponse
                        {
                            Message = $"Blog post with ID: {blogPost.Id} is not published yet. Can not add comments"
                        });
                    }

                    _repositoryService.Create(new BlogPostComment
                    {
                        UserId = currentUser.Id,
                        Comment = model.Comment,
                        BlogPostId = model.BlogPostId,
                        Date = _dateTimeService.UtcNow()
                    });

                    await _repositoryService.SaveAsync();

                    return Ok(new RequestResponse
                    {
                        IsSuccess = true,
                        Message = $"Your comment to post ID: {model.BlogPostId} was added succesfully"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }

                return BadRequest(new RequestResponse
                {
                    Message = "Error submiting comment"
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
