using AutoMapper;
using Blog.Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Blog.Portal.Controllers
{
    public class BlogController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ILogger<BlogController> _logger;
        private readonly HttpClient _httpClient;

        public BlogController(ILogger<BlogController> logger, IMapper mapper, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient("Default");
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> View(int id)
        {
            try
            {
                using var response = await _httpClient.GetAsync($"blogpost?id={id}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var requestReponse = JsonConvert.DeserializeObject<RequestResponse>(content);
                if (requestReponse.IsSuccess && requestReponse.Data != null)
                {
                    var data = JsonConvert.DeserializeObject<List<BlogPostDto>>(requestReponse.Data.ToString());

                    return View(data.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            return View();
        }
    }
}
