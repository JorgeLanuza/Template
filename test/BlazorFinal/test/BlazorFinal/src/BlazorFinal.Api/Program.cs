// using BaseCore.Framework.Web;
using BlazorFinal.Api.Extensions;
using BlazorFinal.Infrastructure;
using BlazorFinal.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// BaseCore Framework
builder.Services.AddBaseCoreWeb(builder.Configuration);
// builder.Services.AddBaseCoreInfrastructure(builder.Configuration);

// Layers
builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer(builder.Configuration);

var app = builder.Build();

// Auto-migration
app.EnsureCreated(); // TODO: Implement strict Clean Arch migration strategy

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
