# Notes API (backend)

ASP.NET Core 8 Web API for the "Notes with tags" feature. Notes are stored **in memory**
(no database) — this is a prototype with no persistence.

## Base URL

The API listens on: **http://localhost:5000**

## Run it

```powershell
cd backend
dotnet run
```

The server binds to `http://localhost:5000` (see `Properties/launchSettings.json`).
Kestrel's default port is also `5000`, so `dotnet run --no-launch-profile` works the same way.

## Endpoints

| Method   | Route              | Description |
|----------|--------------------|-------------|
| `POST`   | `/api/notes`       | Create a note. Body: `{ "title": string, "text": string, "tag": string }`. Returns `201 Created` with the full note including its assigned `id`. |
| `GET`    | `/api/notes`       | Return all notes (`200 OK`, JSON array). |
| `GET`    | `/api/notes?tag=work` | Return notes filtered by tag (case-insensitive exact match). Omit `tag` to get all. |
| `GET`    | `/api/notes/{id}`  | Return one note (`200 OK`) or `404 Not Found`. |
| `DELETE` | `/api/notes/{id}`  | Delete a note (`204 No Content`) or `404 Not Found`. |

## Data model

```json
{
  "id": 1,
  "title": "Buy milk",
  "text": "also eggs",
  "tag": "personal"
}
```

- `id` — integer, auto-increment starting from 1.
- `title` — string, **required** (null/empty/whitespace → `400`).
- `text` — string, optional (may be empty or null).
- `tag` — string, **required** (null/empty/whitespace → `400`).

## Validation & error behavior

- Missing or malformed JSON body → `400`.
- Empty/whitespace `title` or `tag` → `400` with a `ProblemDetails` body (`application/problem+json`).
- `DELETE` on a non-existent id (including a repeated delete) → `404`.
- `GET /api/notes/{id}` on a non-existent id → `404`.

## CORS

CORS is enabled for `http://localhost:5173` and `http://localhost:3000` so the Vite dev
server can call the API from the browser.

## Tests

```powershell
cd backend
dotnet test
```

Integration tests (via `Microsoft.AspNetCore.Mvc.Testing`) cover every acceptance criterion.
