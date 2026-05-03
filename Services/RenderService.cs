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
        // Retrieving info from the DB
        var nodes = dbContext.TemplateNodes
            .OrderBy(node => node.Order)
            .ThenBy(node => node.SectionNumber)
            .ToList();
        
        var processedLines = new List<string>();
        var context = request.Fields;
        var collections = request.Collections;
        var parentsSet = new HashSet<string>();
        
        foreach (var node in nodes)
        {
            var ruleType = node.RuleType;
            var result = "";
            var sectionNumberParent = (node.SectionNumber).Split()[0];

            if (node.ParentId != null && !parentsSet.Contains(sectionNumberParent)) continue;

            result = ruleType switch
            {
                "FillField" => _fillFieldHandler.Handle(node, context),
                "AlternativeField" => _alternativeFieldHandler.Handle(node, context),
                "ConditionalField" => _conditionalFieldHandler.Handle(node, context),
                "OptionalField" => _optionalFieldHandler.Handle(node, context),
                "RepetitiveField" => _repetitiveFieldHandler.Handle(node, collections),
                _ => result
            };

            if (result == "") continue;
            
            // Add the processed text to the processed lines and add parent if needed
            processedLines.Add(node.SectionNumber + ". " + result);
            parentsSet.Add(sectionNumberParent);
            
        }
        var finalText = string.Join(Environment.NewLine, processedLines);
        File.WriteAllText("Data/output.txt", finalText);
        return finalText;
    }
}