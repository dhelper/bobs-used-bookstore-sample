using System.Reflection;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

using Bookstore.Web.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureConfiguration();

builder.ConfigureServices();

builder.ConfigureAuthentication();

builder.ConfigureDependencyInjection();

var app = builder.Build();

await app.ConfigureMiddlewareAsync();

app.MapGet("/health", [AllowAnonymous] () => Results.Json(new
{
    status = "ok",
    version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
}));

app.Run();