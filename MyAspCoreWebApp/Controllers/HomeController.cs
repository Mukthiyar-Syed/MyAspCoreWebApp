using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyAspCoreWebApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MyAspCoreWebApp.Services; // Use your namespace
using Microsoft.AspNetCore.Http;
using Azure.Storage.Files.Shares.Models;
using System.IO;

namespace MyAspCoreWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AzureFileService _azureFileService;

        // COMBINED CONSTRUCTOR
        public HomeController(ILogger<HomeController> logger, AzureFileService azureFileService)
        {
            _logger = logger;
            _azureFileService = azureFileService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    await _azureFileService.UploadFileAsync(file.FileName, stream);
                }
                ViewBag.Message = "File uploaded successfully!";
            }
            else
            {
                ViewBag.Message = "Please select a file to upload.";
            }
            return View("Upload");
        }

        public async Task<IActionResult> ListFiles()
        {
            var files = await _azureFileService.ListFilesAsync();
            var viewModel = new FileViewModel { FileNames = files };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Download(string fileName) // Now accepts filename as a parameter
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound("File name not provided.");
            }

            try
            {
                ShareFileDownloadInfo download = await _azureFileService.DownloadFileAsync(fileName);
                return File(download.Content, download.ContentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file: {FileName}", fileName);
                return NotFound($"File '{fileName}' not found.");
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
