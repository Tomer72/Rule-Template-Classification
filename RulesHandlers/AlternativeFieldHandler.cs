using RuleTemplateClassification.Models;

namespace RuleTemplateClassification.RulesHandlers;

public class AlternativeFieldHandler
{
    private readonly ConditionalFieldHandler _conditionalFieldHandler = new();
    public string Handle(TemplateNode node, Dictionary<string, string?> fields)
    {
        return _conditionalFieldHandler.Handle(node, fields);
    }
}