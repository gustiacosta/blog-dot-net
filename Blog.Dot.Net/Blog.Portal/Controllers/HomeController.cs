using AutoMapper;
using Blog.Application.Interfaces;
using Blog.Application.Models;
using Blog.Domain.Entities;
using Blog.Portal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Blog.Portal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ILogger<HomeController> _logger;
        private readonly IEntityGenericRepository _repositoryService;

        public HomeController(ILogger<HomeController> logger, IEntityGenericRepository repositoryService, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
            _repositoryService = repositoryService;
        }

        public IActionResult Index()
        {
            var data = _repositoryService
                .Get<BlogPost>(
                        c => c.PublishingStatus == (int)Domain.Enums.PostPublishingStatus.Published,
                        c => c.OrderByDescending(c => c.PublishDate), null, null,
                        inc => inc.BlogPostComments)
                .ToList();

            var posts = _mapper.Map<IEnumerable<BlogPostDto>>(data);

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
