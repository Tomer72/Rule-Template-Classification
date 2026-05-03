using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RuleTemplateClassification.Models;

namespace RuleTemplateClassification.Data;

public static class DatabaseSetup
{
    public static void Initialize(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        if (db.TemplateNodes.Any()) return;
        
        var json = File.ReadAllText("Data/seed-data2.json");
        var nodes = JsonSerializer.Deserialize<List<TemplateNode>>(json, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });
        
        if (nodes == null) return;
        db.TemplateNodes.AddRange(nodes);
        db.SaveChanges();
    }
}