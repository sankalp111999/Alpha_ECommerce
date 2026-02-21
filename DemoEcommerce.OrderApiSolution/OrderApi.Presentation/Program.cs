using OrderApi.Infrastructure.DependencyInjection;
using OrderApi.Application.DependencyInjection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddInfrastructureService(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddApplicationService(builder.Configuration);

var app = builder.Build();

app.MapControllers();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UserInfrastructurePolicy();
app.UseHttpsRedirection();
app.UseAuthorization();

app.Run();
