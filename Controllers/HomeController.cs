using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model; 
using AwsS3_UploadsMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http.Handlers;
using System.Net.Http;
using AwsS3_UploadsMVC.Interfaces;

namespace AwsS3_UploadsMVC.Controllers
{
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly IAwsS3Upload _awsS3UploadService;        
        public HomeController(ILogger<HomeController> logger, IAwsS3Upload awsS3UploadService)
        {
            _logger = logger;
            _awsS3UploadService = awsS3UploadService;            
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UploadFileToS3Async(IFormFile file)
        {
            EtagModel etagModellist = new EtagModel();
            etagModellist.parts = new List<Part1>();
            if (file == null || file.Length == 0)
            {
                // Handle the case where no file was provided or it has zero length.
                return BadRequest("File not provided or empty.");
            }
            else {
                _awsS3UploadService.AwsS3Upload(file);
            }
            return View("Index");
        }
    }
}