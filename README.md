# Rule Template Classification

Rule Template Classification is an ASP.NET Core Web API that renders a tender document from rule-based template data.

The application receives structured JSON input, loads template sections from a local SQLite database, applies rule handlers, and returns the final rendered text. It is designed as a focused demonstration of dynamic document generation using fill rules, conditional rules, alternative text rules, optional sections, and repeated collection sections.

## Tech Stack

- .NET 10
- ASP.NET Core Minimal API
- Entity Framework Core
- SQLite

## What The Project Does

The API exposes one endpoint:

```http
POST /render
```

The request body contains:

- `fields`: single-value inputs used for placeholders and conditions.
- `collections`: repeated data groups used by repetitive sections.

The response is a plain-text rendered document, An example rendered result is included in:

```text
Data/example-output.txt
```

## Project Structure

```text
RuleTemplateClassification/
├── Data/
│   ├── AppDbContext.cs
│   ├── DatabaseSetup.cs
│   ├── seed-data2.json
│   ├── sample-input2.json
│   ├── example-output.txt
│   
├── Models/
│   ├── RenderRequest.cs
│   └── TemplateNode.cs
├── RulesHandlers/
│   ├── FillFieldHandler.cs
│   ├── ConditionalFieldHandler.cs
│   ├── AlternativeFieldHandler.cs
│   ├── OptionalFieldHandler.cs
│   └── RepetitiveFieldHandler.cs
├── Services/
│   └── RenderService.cs
├── Program.cs
├── RuleTemplateClassification.csproj
└── RuleTemplateClassification.http
```

## Runtime Flow

1. `Program.cs` receives a `POST /render` request.
2. ASP.NET Core binds the body to `RenderRequest`.
3. `RenderService` loads template nodes from SQLite.
4. Nodes are ordered by `Order`, then by `SectionNumber`.
5. Each node is sent to the handler matching its `RuleType`.
6. Empty handler results are skipped.
7. Non-empty results are prefixed with the section number.
8. The final text is returned and written to `Data/output.txt`.

## Request Example

The active HTTP client file sends:

```http
POST http://localhost:5225/render
Content-Type: application/json

< Data/sample-input2.json
```

The request shape is:

```json
{
  "fields": {
    "OfficeName": "משרד האוצר",
    "DepartmentName": "אגף מערכות מידע",
    "BidNumber": "123/2026",
    "BidName": "שירותי תוכנה",
    "IsIntroEdited": "false",
    "IsBasket": "true",
    "IsSingleWinner": "false",
    "WinnerCount": "3",
    "DurationMonths": "24",
    "ExtensionMonths": "12",
    "TenderBoxType": "Regular"
  },
  "collections": {
    "Baskets": [
      {
        "BasketNumber": "1",
        "BasketName": "פיתוח מערכות",
        "BasketScopeInShekels": "250,000",
        "BasketOptionInShekels": "50,000"
      }
    ]
  }
}
```

The full sample request is available at:

```text
Data/sample-input2.json
```

## Template Data

Template nodes are seeded from:

```text
Data/seed-data2.json
```

Each node describes one rendered section:

```json
{
  "id": 17,
  "sectionNumber": "1.16",
  "parentId": null,
  "order": 17,
  "ruleType": "RepetitiveField",
  "textPlaceholder": "סל {{BasketNumber}} - {{BasketName}} | היקף התקשרות מירבי: {{BasketScopeInShekels}} ש\"ח | זכות ברירה: {{BasketOptionInShekels}}",
  "fieldName": "Baskets",
  "conditionFieldName": null,
  "expectedValue": null
}
```

### Template Node Fields

`Id`

Numeric identifier stored in the database.

`SectionNumber`

The section number printed before the rendered text, for example `1.16`.

`ParentId`

Optional parent node id. `RenderService` currently uses this to skip child nodes whose parent was not rendered.

`Order`

Controls rendering order.

`RuleType`

Determines which handler processes the node.

`TextPlaceholder`

The template text. Placeholders use double curly braces, for example `{{OfficeName}}`.

`FieldName`

Used by optional and repetitive rules.

`ConditionFieldName`

Used by conditional and alternative rules.

`ExpectedValue`

The string value required for a condition to pass.

## Rule Types

### FillField

Replaces placeholders in `TextPlaceholder` with matching values from `fields`.

Example:

```text
מכרז {{BidNumber}}
```

becomes:

```text
מכרז 123/2026
```

### ConditionalField

Renders only when:

```text
fields[ConditionFieldName] == ExpectedValue
```

Example:

```json
{
  "ruleType": "ConditionalField",
  "conditionFieldName": "IsBasket",
  "expectedValue": "true"
}
```

This renders only when the request contains:

```json
{
  "fields": {
    "IsBasket": "true"
  }
}
```

### AlternativeField

Uses the same condition logic as `ConditionalField`.

This is useful for mutually exclusive text variants. For example, one node can render when `IsSingleWinner` is `"true"`, and another can render when `IsSingleWinner` is `"false"`.

### OptionalField

Renders only when `fields` contains the node's `FieldName`.

Example:

```json
{
  "ruleType": "OptionalField",
  "fieldName": "ExtensionMonths"
}
```

If `ExtensionMonths` exists in the request, the section is rendered. If it is missing, the section is skipped.

### RepetitiveField

Renders once for each item in a named collection.

Example node:

```json
{
  "ruleType": "RepetitiveField",
  "fieldName": "Baskets",
  "textPlaceholder": "סל {{BasketNumber}} - {{BasketName}}"
}
```

Example request:

```json
{
  "collections": {
    "Baskets": [
      {
        "BasketNumber": "1",
        "BasketName": "פיתוח מערכות"
      },
      {
        "BasketNumber": "2",
        "BasketName": "תחזוקת מערכות"
      }
    ]
  }
}
```

The handler renders one line per collection item.

## Database Behavior

This project currently uses:

```csharp
db.Database.EnsureCreated();
```

That means the SQLite database schema is created directly from the current EF Core model when the database does not exist.

The app does not currently rely on EF migrations.

After creating the schema, `DatabaseSetup` seeds template nodes from:

```text
Data/seed-data2.json
```

Seeding only happens when the `TemplateNodes` table is empty:

```csharp
if (db.TemplateNodes.Any()) return;
```

## Updating Seed Data

Because seeding only runs when `TemplateNodes` is empty, editing `Data/seed-data2.json` does not automatically update an existing SQLite database.

For local development, the simplest refresh flow is:

1. Stop the application.
2. Delete the local SQLite files:

```text
app.db
app.db-shm
app.db-wal
```

3. Run the application again:

```bash
dotnet run
```

`EnsureCreated()` will recreate the database, and the current `seed-data2.json` will be inserted.

Alternatively, clear only the seeded table:

```sql
DELETE FROM TemplateNodes;
```

Then restart the app so the seed file is inserted again.

## Running The Project

From the project directory:

```bash
dotnet restore
dotnet build
dotnet run
```

The HTTP profile listens on:

```text
http://localhost:5225
```

Send a request with curl:

```bash
curl -X POST http://localhost:5225/render \
  -H "Content-Type: application/json" \
  --data-binary @Data/sample-input2.json
```

Or run the request from:

```text
RuleTemplateClassification.http
```

## Example Output

The repository includes a real example output at:

```text
Data/example-output.txt
```

Excerpt:

```text
1. מדינת ישראל - משרד האוצר
1.1. אגף: אגף מערכות מידע
1.2. מכרז 123/2026
1.3. לשירותי תוכנה
1.8. מכרז זה הוא "מכרז סלים" בו המציעים יכולים להגיש את הצעתם לחלקים שונים של המכרז, כמפורט במסגרת מסמכי המכרז.
1.10. המזמין רשאי לבחור עד 3 זוכים במכרז.
1.16. סל 1 - פיתוח מערכות | היקף התקשרות מירבי: 250,000 ש"ח | זכות ברירה: 50,000
סל 2 - תחזוקת מערכות | היקף התקשרות מירבי: 150,000 ש"ח | זכות ברירה: ללא
סל 3 - בדיקות ואבטחת איכות | היקף התקשרות מירבי: 100,000 ש"ח | זכות ברירה: 25,000
1.19. המועד האחרון להגשת הצעות במכרז הוא בתאריך 30/06/2026 בשעה 14:00.
```

## Notes And Limitations

- Condition values are compared as strings, so use `"true"` / `"false"` instead of JSON booleans.
- `EnsureCreated()` is convenient for this assignment/demo, but it is not a migration strategy for production systems.
- `Data/output.txt` is generated on every render request.
- `Data/example-output.txt` is a committed example result.
- Local SQLite database files should not be committed.
- The current response format is plain text, not DOCX or PDF.

