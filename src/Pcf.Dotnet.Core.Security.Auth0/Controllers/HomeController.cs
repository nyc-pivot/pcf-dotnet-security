﻿using Microsoft.AspNetCore.Mvc;
using Pcf.Dotnet.Core.Security.Auth0.Models;
using System.Diagnostics;

namespace Pcf.Dotnet.Core.Security.Auth0.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
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