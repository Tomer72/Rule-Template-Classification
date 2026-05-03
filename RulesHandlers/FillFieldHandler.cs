using RuleTemplateClassification.Models;

namespace RuleTemplateClassification.RulesHandlers;

public class FillFieldHandler
{
    public string Handle(TemplateNode node, Dictionary<string, string?> fields)
    {
        string text = node.TextPlaceholder;
        foreach (var (key, value) in fields)
        {
            var placeholder = "{{" + key + "}}";
            if (!text.Contains(placeholder)) continue;
            
            text = text.Replace(placeholder, value ?? "");

            if (!text.Contains("{{"))
            {
                break;
            }
        }
        return text;
    }
}