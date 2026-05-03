using RuleTemplateClassification.Data;
using RuleTemplateClassification.Services;
using RuleTemplateClassification.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Setting up the database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<RenderService>();

var app = builder.Build();

DatabaseSetup.Initialize(app);

// Post request to the RenderService, starting the classification
app.MapPost("/render", (RenderRequest request, RenderService renderService) =>
{
    var result = renderService.ProcessInput(request);
    return Results.Ok(result);
});

app.Run();

    