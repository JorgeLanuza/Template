// using BaseCore.Framework.Web;
using VerificationProject.Infrastructure;
using VerificationProject.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// BaseCore Framework
// builder.Services.AddBaseCoreWeb(builder.Configuration);

// Layers
builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer(builder.Configuration);

// Identity Server (OpenIddict)
// builder.Services.AddBaseCoreIdentityServer(builder.Configuration);

var app = builder.Build();

// Auto-migration
// app.EnsureCreated();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
