using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace inbox_app.Controllers
{
    public class HomeController : Controller
    {
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