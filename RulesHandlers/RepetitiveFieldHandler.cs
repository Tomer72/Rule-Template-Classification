using RuleTemplateClassification.Models;

namespace RuleTemplateClassification.RulesHandlers;

public class RepetitiveFieldHandler
{
    private readonly FillFieldHandler _fillFieldHandler = new();
    public string Handle(TemplateNode node,
        Dictionary<string, List<Dictionary<string, string?>>> collections)
    {
        if (node.FieldName == null ||
            !collections.TryGetValue(node.FieldName, out var fieldsCollection))
        {
            return "";
        }

        var result = new List<string>();

        foreach (var fields in fieldsCollection)
        {
            result.Add(_fillFieldHandler.Handle(node, fields));
        }
        
        return string.Join(Environment.NewLine, result);
    }
}