namespace RuleTemplateClassification.Models;
// Data type that defines each template node data structure on the DB.
public class TemplateNode
{
    public int Id { get; set; }
    public string SectionNumber { get; set; } = "";
    public int? ParentId { get; set; }
    
    public string RuleType { get; set; } = "";
    
    public string TextPlaceholder { get; set; } = "";
    
    public string? FieldName { get; set; } // Optional/Repeat
    
    public string? ConditionFieldName { get; set; } // Conditional / Alternative
    
    public string? ExpectedValue { get; set; }
    
    public int Order { get; set; }
}
