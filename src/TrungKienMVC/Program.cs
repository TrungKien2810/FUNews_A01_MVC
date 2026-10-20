using Microsoft.AspNetCore.Authentication.Cookies;
using Repositories;
using Services;
using TrungKienMVC.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// On Windows the default host adds an EventLog provider that needs admin rights
// to create/open the ".NET Runtime" source. Without elevation every Kestrel
// warning throws inside the logger and kills the request. Console+Debug is
// enough for this assignment.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllersWithViews();

builder.Services.Configure<AdminAccountOptions>(builder.Configuration.GetSection(AdminAccountOptions.SectionName));

// Repositories (Repository -> DAO singleton -> DbContext)
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<INewsArticleRepository, NewsArticleRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();

// Services (Controller -> Service -> Repository)
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<INewsArticleService, NewsArticleService>();
builder.Services.AddScoped<ITagService, TagService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

// Compress HTML, CSS, JS and the RSS feed on the way out. The feed is not in the default
// list because its content type is application/rss+xml, not one of the text/* defaults.
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes
        .Concat(new[] { "application/rss+xml", "image/svg+xml" });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// A 404 from a mistyped address is a page, not a blank screen.
app.UseStatusCodePagesWithReExecute("/Home/ErrorPage", "?code={0}");

// Baseline hardening. No CSP yet: the layout carries one inline script that marks the
// document script-capable before first paint, and a policy without a nonce for it would
// either break the menu or be written as 'unsafe-inline' and mean nothing.
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["X-Frame-Options"] = "SAMEORIGIN";
    await next();
});

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    // Our own css/js are requested with asp-append-version, and the vendored libraries
    // change only when the folder does, so a week is a safe cache lifetime for both.
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers.CacheControl = "public,max-age=604800";
    }
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// The Login page is the default page of the application.
app.MapControllerRoute(
    name: "login",
    pattern: "",
    defaults: new { controller = "Account", action = "Login" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.Run();
