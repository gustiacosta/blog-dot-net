﻿using AutoMapper;
using Blog.Application.Interfaces;
using Blog.Application.Models;
using Blog.Portal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Blog.Portal.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, IMapper mapper, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient("Default");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                using var response = await _httpClient.GetAsync("blogpost");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var requestReponse = JsonConvert.DeserializeObject<RequestResponse>(content);
                if (requestReponse.IsSuccess && requestReponse.Data != null)
                {
                    var data = JsonConvert.DeserializeObject<List<BlogPostDto>>(requestReponse.Data.ToString());

                    return View(data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

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
