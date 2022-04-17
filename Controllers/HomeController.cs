using Cassandra;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace inbox_app.Controllers
{
    public class HomeController : Controller
    {
        private IWebHostEnvironment _webHostEnvironment;

        public HomeController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
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
        public async Task<IActionResult> SignInAsync()
        {
            ViewBag.ReturnUrl = "/";
            return View(await GetExternalProvidersAsync(HttpContext));
        }

        [HttpPost]
        public async Task<IActionResult> SignInAsync([FromForm] string provider)
        {
            if (string.IsNullOrWhiteSpace(provider))
            {
                return BadRequest();
            }

            return await IsProviderSupportedAsync(HttpContext, provider) is false
                ? BadRequest()
                : Challenge(new AuthenticationProperties
                {
                    RedirectUri = Url.IsLocalUrl(ViewBag.ReturnUrl) ? ViewBag.ReturnUrl : "/"
                }, provider);
        }


        [HttpGet]
        public IActionResult Connect()
        {
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "db", "secure-connect-inbox-app-db.zip");

            var cluster = Cluster.Builder()
                .WithCloudSecureConnectionBundle(path)
                .WithCredentials("NOpZWGaMNZLOQkzWLhGTBxMu", "Ov-lG08,Lz6Nlb1BK9,,XUut3p5GeQsmxCczRXaaavPJIfCJaiUcZ.Fd813DLZvYe4lbZgUPBcn14Y2RtOgh.oDl3uaFWTIbYFY0yhn_aQR7JMd3C5w0OPmfO8NvFayE")
                .Build();

            var session = cluster.Connect("main");


            return null;
        }




        private static async Task<AuthenticationScheme[]> GetExternalProvidersAsync(HttpContext context)
        {
            var schemes = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
            return (await schemes.GetAllSchemesAsync())
                .Where(scheme => !string.IsNullOrEmpty(scheme.DisplayName))
                .ToArray();
        }

        private static async Task<bool> IsProviderSupportedAsync(HttpContext context, string provider) =>
            (await GetExternalProvidersAsync(context))
            .Any(scheme => string.Equals(scheme.Name, provider, StringComparison.OrdinalIgnoreCase));
    }
}