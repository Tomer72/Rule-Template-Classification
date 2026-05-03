namespace RuleTemplateClassification.Models;

public class RenderRequest
{
    public Dictionary<string, string?> Fields { get; set; } = new();
    public Dictionary <string, List<Dictionary<string, string?>>> Collections { get; set; } = new();
}