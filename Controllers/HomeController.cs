using Cassandra;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using inbox_app.Options;

namespace inbox_app.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AstraDbConnectOptions _astraDbOptions;

        public HomeController(IWebHostEnvironment webHostEnvironment, IOptionsSnapshot<AstraDbConnectOptions> astraDbOptions)
        {
            _webHostEnvironment = webHostEnvironment;
            _astraDbOptions = astraDbOptions.Value;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (HttpContext!.User.Identity?.IsAuthenticated == true)
            {
                var claims = HttpContext.User.Claims.ToList();
                ViewBag.UserName = claims.FirstOrDefault(x => x.Type == "urn:github:name")!.Value;
            }

            return View();
        }

        [HttpGet]
        public IActionResult Connect()
        {
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "db", "secure-connect-inbox-app-db.zip");

            var cluster = Cluster.Builder()
                .WithCloudSecureConnectionBundle(path)
                .WithCredentials(_astraDbOptions.UserName, _astraDbOptions.Password)
                .Build();

            var session = cluster.Connect("main");

            return null;
        }
    }
}