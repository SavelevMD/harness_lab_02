# Notes with tags — frontend

A React + Vite single-page app for creating, listing, filtering, and deleting
tagged notes. It talks to the backend API described below.

## Requirements

- Node.js 18+ (tested with Node 24)
- npm

## Setup

```bash
cd frontend
npm install
```

## Run the dev server

```bash
npm run dev
```

Vite serves the app at `http://localhost:5173` by default.

## Pointing at the backend

The API base URL defaults to `http://localhost:5000`. Override it with the
`VITE_API_BASE_URL` environment variable:

```bash
# PowerShell
$env:VITE_API_BASE_URL="http://localhost:5000"; npm run dev

# bash / zsh
VITE_API_BASE_URL=http://localhost:5000 npm run dev
```

The value is read at build/dev time via `import.meta.env.VITE_API_BASE_URL`.

## API contract

- `POST /api/notes` — body `{ title, text, tag }` → `201` created note, `400` on validation error.
- `GET /api/notes` — `200` array of all notes.
- `GET /api/notes?tag=work` — `200` array filtered by tag.
- `DELETE /api/notes/{id}` — `204` on success, `404` if not found.

## Scripts

- `npm run dev` — start the dev server.
- `npm run build` — production build into `dist/`.
- `npm test` — run the unit/integration test suite (Vitest + React Testing Library).

## Tests

The test suite covers every acceptance criterion: form rendering, note listing,
tag filtering (re-fetch with `?tag=`), deletion with refresh, validation
(disabled submit / inline errors), and loading/error states.
