using RuleTemplateClassification.Models;

namespace RuleTemplateClassification.RulesHandlers;
public class OptionalFieldHandler
{
    private readonly FillFieldHandler _fillFieldHandler = new();
    public string Handle(TemplateNode node, Dictionary<string, string?> fields)
    {
        var fieldName = node.FieldName;
        return fields.TryGetValue(fieldName!, out var fieldValue) ?
            _fillFieldHandler.Handle(node, fields) : "";
    }
}