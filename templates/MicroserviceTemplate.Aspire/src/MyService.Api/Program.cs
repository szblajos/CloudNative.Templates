using MyService.Api.Common.Extensions;
using MyService.Application.Extensions;
using Scalar.AspNetCore;
using MyService.Infrastructure.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOpenApi();

// Register infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration, "DefaultConnection");

// Register repositories
builder.Services.AddScoped<MyService.Domain.Items.Interfaces.IItemsRepository, MyService.Infrastructure.Items.Repositories.ItemsRepository>();
builder.Services.AddScoped<MyService.Domain.Common.Interfaces.IOutboxMessageRepository, MyService.Infrastructure.Common.Repositories.OutboxMessageRepository>();

// Add ServiceDefaults (if using Aspire)
builder.AddServiceDefaults();

// Register application services
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Configure OpenAPI
    app.MapOpenApi()
        .WithOpenApi()
        .WithTags("OpenAPI")
        .WithDescription("OpenAPI documentation for MyService API");

    // Configure Scalar API
    // The scalar API is used for querying and manipulating data
    // It provides a flexible and efficient way to work with your data models
    app.MapScalarApiReference();

    // Only for non-persistent databases
    // For testing purposes only!
    // If you use a persistent database, remove this section
    // and use migrations 
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<MyService.Infrastructure.Data.AppDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure endpoints
app.AddEndpoints();

app.UseHttpsRedirection();

app.Run();

public partial class Program { } // This is a hack for minimal APIs. We need to add this at the end of the file to expose Program for testing
