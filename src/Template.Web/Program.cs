using BaseCore.Framework.Security.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using MudBlazor.Services;

using Template.Application;
using Template.Infrastructure;
using Template.Infrastructure.Identity;
using Template.Web.Components;
using Template.Web.Extensions;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer(builder.Configuration);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\mssqllocaldb;Database=BaseCoreTemplateDb;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddBaseCoreIdentityServer<AppDbContext>(builderOptions =>
{
	OpenIddictCoreBuilder coreBuilder = (OpenIddictCoreBuilder)builderOptions;
	coreBuilder.UseEntityFrameworkCore().UseDbContext<AppDbContext>();
});

// Auth is already configured by AddBaseCoreIdentityServer

WebApplication? app = builder.Build();

app.EnsureCreated();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// app.MapIdentityEndpoints(); // Revisit if Minimal API endpoints are still needed with Blazor Auth

await app.RunAsync();
