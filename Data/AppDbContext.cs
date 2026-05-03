using Microsoft.EntityFrameworkCore;
using RuleTemplateClassification.Models;

namespace RuleTemplateClassification.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TemplateNode> TemplateNodes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TemplateNode>()
            .HasKey(t => t.SectionNumber);
    }
}