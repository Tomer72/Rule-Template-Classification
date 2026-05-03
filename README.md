# Rule Template Classification

Rule Template Classification is a small ASP.NET Core Web API that renders a text document from template rules stored in SQLite. The API receives structured input, loads ordered template nodes from the database, applies rule handlers, and returns the rendered output.

The project is intended to demonstrate a rule-based rendering flow for document templates, including filled fields, conditional sections, alternative sections, optional sections, and repeated collection sections.

## Tech Stack

- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- .NET 10

## Project Structure

```text
RuleTemplateClassification/
├── Data/
│   ├── AppDbContext.cs
│   ├── DatabaseSetup.cs
│   ├── sample-input.json
│   ├── seed-data.json
│   └── output.txt
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
└── RuleTemplateClassification.http
```

## How It Works

1. The API receives a `POST /render` request.
2. The request body is bound to `RenderRequest`.
3. `RenderService` loads all `TemplateNode` rows from SQLite, ordered by `Order` and `SectionNumber`.
4. Each node is routed to the matching rule handler according to `RuleType`.
5. Non-empty rendered lines are joined into the final output.
6. The rendered output is returned in the HTTP response and also written to `Data/output.txt`.

## Request Format

The API expects a JSON object with two top-level sections:

```json
{
  "fields": {
    "OfficeName": "Example Office",
    "BidNumber": "123/2026",
    "BidName": "Software Services",
    "IsBasket": "true",
    "IsSingleWinner": "true",
    "WinnerCount": "3",
    "ExtensionMonths": "12"
  },
  "collections": {
    "Baskets": [
      {
        "BasketNumber": "1",
        "BasketName": "Development",
        "BasketScopeInShekels": "250,000"
      },
      {
        "BasketNumber": "2",
        "BasketName": "Maintenance",
        "BasketScopeInShekels": "150,000"
      }
    ]
  }
}
```

`fields` is used for regular placeholders and boolean-like conditions.

`collections` is used for repeated sections. Each collection item is a dictionary of placeholders for one repeated rendered line.

Important: condition values are currently compared as strings. Use `"true"` and `"false"` rather than JSON booleans `true` and `false`.

## Template Seed Format

Template rules are loaded from `Data/seed-data.json` into the `TemplateNodes` table.

Each template node contains:

```json
{
  "id": 1,
  "sectionNumber": "2.1",
  "parentId": null,
  "order": 1,
  "ruleType": "FillField",
  "textPlaceholder": "The office {{OfficeName}} published bid {{BidNumber}}.",
  "fieldName": null,
  "conditionFieldName": null,
  "expectedValue": null
}
```

### Rule Types

`FillField`

Replaces placeholders in `textPlaceholder` using values from `fields`.

Example:

```text
{{OfficeName}}
```

is replaced with:

```text
Example Office
```

`ConditionalField`

Renders the node only when `fields[conditionFieldName]` equals `expectedValue`.

Example:

```json
{
  "ruleType": "ConditionalField",
  "conditionFieldName": "IsBasket",
  "expectedValue": "true"
}
```

`AlternativeField`

Uses the same comparison behavior as `ConditionalField`. It is useful when two template nodes represent mutually exclusive alternatives, such as `IsSingleWinner = "true"` and `IsSingleWinner = "false"`.

`OptionalField`

Renders the node only when `fields` contains the configured `fieldName`.

`RepetitiveField`

Uses `fieldName` as the collection name, then renders the node once for each item in that collection.

Example:

```json
{
  "ruleType": "RepetitiveField",
  "fieldName": "Baskets",
  "textPlaceholder": "Basket {{BasketNumber}} - {{BasketName}}"
}
```

with:

```json
{
  "collections": {
    "Baskets": [
      { "BasketNumber": "1", "BasketName": "Development" },
      { "BasketNumber": "2", "BasketName": "Maintenance" }
    ]
  }
}
```

## Running Locally

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

Send a request:

```bash
curl -X POST http://localhost:5225/render \
  -H "Content-Type: application/json" \
  --data-binary @Data/sample-input.json
```

You can also use `RuleTemplateClassification.http` from Rider or another IDE HTTP client.

## Database Setup

The app uses SQLite through Entity Framework Core. On startup, `DatabaseSetup.Initialize` runs:

```csharp
db.Database.Migrate();
```

Then it inserts rows from `Data/seed-data.json` only if the `TemplateNodes` table is empty.

If you do not have migrations yet, create one:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

If `dotnet ef` is not installed:

```bash
dotnet tool install --global dotnet-ef
```

## Updating Seed Data

Changes to `Data/seed-data.json` are not automatically synchronized into an existing database, because seeding stops when `TemplateNodes` already contains rows.

For local development, use one of these approaches:

Clear the table and restart:

```sql
DELETE FROM TemplateNodes;
```

Or delete the local SQLite files and recreate the database:

```text
app.db
app.db-shm
app.db-wal
```

Then run:

```bash
dotnet ef database update
dotnet run
```

## Example Output

The API returns a rendered plain-text document. A typical output looks like:

```text
1. State of Israel - Example Office
2.1. The office Example Office published bid 123/2026 named Software Services.
2.2. This bid contains baskets.
2.3. The buyer may choose a single winner.
2.5. The buyer may extend the agreement by 12 months.
2.6. Basket 1 - Development, scope: 250,000
Basket 2 - Maintenance, scope: 150,000
```

The current sample data may contain Hebrew text. The rendering logic is language-agnostic and works by replacing placeholder tokens.

## Troubleshooting

`SQLite Error 1: no such table: TemplateNodes`

The database exists but the schema was not created. Create and apply migrations:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

`seed-data.json` changed but output did not change

The database still contains old rows. Clear `TemplateNodes` or delete the local SQLite files, then restart the app.

`RepetitiveField` returns an empty line

Check that the database row has a `fieldName` matching a key under `collections`. For example, `fieldName = "Baskets"` requires:

```json
{
  "collections": {
    "Baskets": []
  }
}
```

`ConditionalField` does not render

Check that `fields` contains the exact condition key and value. Values are compared as strings, so `"true"` is different from `true`.

## Notes

- `Data/output.txt` is generated by the app and should not be treated as source data.
- Local SQLite database files should not be committed.
- `Data/sample-input.json` and `Data/seed-data.json` are part of the demo and should be committed.
