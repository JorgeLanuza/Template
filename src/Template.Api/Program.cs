using BaseCore.Framework.Security.Identity;

using Template.Application;
using Template.Infrastructure;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer(builder.Configuration);

// Auth Validation
builder.Services.AddBaseCoreIdentityValidation();

// End of services

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<BaseCore.Framework.Web.Middlewares.ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
await app.RunAsync();
