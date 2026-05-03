using RuleTemplateClassification.Models;

namespace RuleTemplateClassification.RulesHandlers;

public class ConditionalFieldHandler
{
    private readonly FillFieldHandler _fillFieldHandler = new();
    
    public string Handle(TemplateNode node, Dictionary<string, string?> fields)
    {
        var condition = node.ConditionFieldName;
        
        if (condition == null ||
            !fields.TryGetValue(condition, out var actualValue) ||
            actualValue != node.ExpectedValue)
        {
            return "";
        }

        return _fillFieldHandler.Handle(node, fields);

    }
}