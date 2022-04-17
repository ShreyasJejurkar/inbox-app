using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(x =>
{
    x.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(x =>
{
    x.LoginPath = "/home/signin";
    x.LogoutPath = "/home/signout";
}).AddGitHub(x =>
{
    x.ClientId = builder.Configuration["github:clientId"]!;
    x.ClientSecret = builder.Configuration["github:clientSecret"]!;
    x.CallbackPath = "/home/signin-github";

    x.Scope.Add("read:user");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/home/signout", async (httpContext) =>
{
    await httpContext.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new AuthenticationProperties()
        {
            RedirectUri = "/"
        }
        );
});

app.Run();
