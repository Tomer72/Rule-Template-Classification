using RuleTemplateClassification.Data;
using RuleTemplateClassification.Models;
using RuleTemplateClassification.RulesHandlers;

namespace RuleTemplateClassification.Services;

public class RenderService(AppDbContext dbContext)
{
    private readonly FillFieldHandler _fillFieldHandler = new();
    private readonly AlternativeFieldHandler _alternativeFieldHandler = new();
    private readonly ConditionalFieldHandler _conditionalFieldHandler = new();
    private readonly OptionalFieldHandler _optionalFieldHandler = new();
    private readonly RepetitiveFieldHandler _repetitiveFieldHandler = new();

    public string ProcessInput(RenderRequest request)
    {
        var nodes = dbContext.TemplateNodes
            .OrderBy(node => node.Order)
            .ToList();

        var processedLines = new List<string>();
        var context = request.Fields;
        var collections = request.Collections;
        var renderedIds = new HashSet<int>();

        foreach (var node in nodes)
        {
            // Skip child nodes whose parent did not render
            if (node.ParentId.HasValue && !renderedIds.Contains(node.ParentId.Value)) continue;

            var result = node.RuleType switch
            {
                "FillField" => _fillFieldHandler.Handle(node, context),
                "AlternativeField" => _alternativeFieldHandler.Handle(node, context),
                "ConditionalField" => _conditionalFieldHandler.Handle(node, context),
                "OptionalField" => _optionalFieldHandler.Handle(node, context),
                "RepetitiveField" => _repetitiveFieldHandler.Handle(node, collections),
                _ => ""
            };

            if (result == "") continue;

            processedLines.Add(node.SectionNumber + ". " + result);
            renderedIds.Add(node.Id);
        }
        var finalText = string.Join(Environment.NewLine, processedLines);
        File.WriteAllText("Data/output.txt", finalText);
        return finalText;
    }
}