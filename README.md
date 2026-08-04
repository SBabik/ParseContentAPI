# ParseContentApi (obviously supported by AI tools and thoroughly composed)

## Project structure

```
ParseContentApi.sln
src/ParseContentApi/
  Program.cs                        application setup, DI, global error handling
  Endpoints/ParseContentEndpoint.cs definition of POST /api/v1/parse-content
  Models/
    ContentType.cs                  enum CSV / INTERNAL_JSON
    ParseRequest.cs                 request DTO { type, content }
    ParseResponse.cs                success and error response DTOs
  Json/ContentTypeJsonConverter.cs  maps "CSV"/"INTERNAL_JSON" to and from the enum
  Services/
    IContentParser.cs               shared parser contract
    CsvContentParser.cs             CSV parsing logic
    CsvTokenizer.cs                 low level CSV tokenizer (core parts of RFC 4180)
    CsvValueConverter.cs            guesses the type of a CSV cell (int/double/bool/string/null)
    InternalJsonContentParser.cs    validation and deserialization of INTERNAL_JSON
    ContentParserFactory.cs         factory selecting a parser based on the type
  Exceptions/ParsingException.cs    controlled domain error, mapped to 400
tests/ParseContentApi.Tests/        unit tests for the parsers plus endpoint integration tests (xUnit)
samples/                            ready made JSON payloads for manual testing (curl / Postman)
```

The API was designed to allow for offline execution without external dependencies.

## Running the API

```bash
cd src/ParseContentApi
dotnet run
# The API listens on http://localhost:5080 by default
```

## Running the tests

```bash
dotnet test
```

## Example requests

### CSV

```bash
curl -X POST http://localhost:5080/api/v1/parse-content \
  -H "Content-Type: application/json" \
  -d @samples/sample_csv_request.json
```

The decoded `content` value is:

```csv
name,age,city
Alicja,30,Warszawa
Bartek,25,Krakow
```

Response:

```json
{
  "status": "success",
  "type": "CSV",
  "count": 2,
  "data": [
    { "name": "Alicja", "age": 30, "city": "Warszawa" },
    { "name": "Bartek", "age": 25, "city": "Krakow" }
  ]
}
```

### INTERNAL_JSON

```bash
curl -X POST http://localhost:5080/api/v1/parse-content \
  -H "Content-Type: application/json" \
  -d @samples/sample_internal_json_request.json
```

The decoded `content` value is:

```json
[
  { "id": 1, "email": "a@example.com", "active": true },
  { "id": 2, "email": "b@example.com", "active": false }
]
```

Response:

```json
{
  "status": "success",
  "type": "INTERNAL_JSON",
  "count": 2,
  "data": [
    { "id": 1, "email": "a@example.com", "active": true },
    { "id": 2, "email": "b@example.com", "active": false }
  ]
}
```

### Error examples (400 Bad Request)

| Situation                                                | Example `message` in `ErrorResponse`                                   |
| -------------------------------------------------------- | ---------------------------------------------------------------------- |
| Missing `type` field                                     | `Field 'type' is required. Allowed values: CSV, INTERNAL_JSON.`        |
| Unsupported `type`, for example `"XML"`                  | `Unsupported content type: 'XML'. Allowed values: CSV, INTERNAL_JSON.` |
| Empty or missing `content`                               | `Field 'content' is required and cannot be empty.`                     |
| `content` is not valid Base64                            | `Field 'content' does not contain validly encoded Base64 data.`        |
| CSV: a row with the wrong number of columns              | `Row 3 has 2 columns, expected 3 based on the header.`                 |
| CSV: a duplicate column in the header                    | `The CSV header contains a duplicate column: 'a'.`                     |
| INTERNAL_JSON: invalid JSON                              | `Invalid JSON in field 'content': ...`                                 |
| INTERNAL_JSON: an array of primitives instead of objects | `Element at index 0 is not a JSON object (received: Number).`          |

The error response always has the same shape:

```json
{ "status": "error", "message": "..." }
```
