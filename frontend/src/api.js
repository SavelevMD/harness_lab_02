const BASE_URL =
  import.meta.env.VITE_API_BASE_URL || "http://localhost:5000";

async function parseError(res, fallback) {
  try {
    const data = await res.json();
    if (data && data.message) return data.message;
  } catch {
    // ignore non-JSON error bodies
  }
  return fallback;
}

export async function getNotes() {
  const res = await fetch(`${BASE_URL}/api/notes`);
  if (!res.ok) {
    throw new Error(await parseError(res, `Failed to load notes (${res.status})`));
  }
  return res.json();
}

export async function getNotesByTag(tag) {
  const res = await fetch(
    `${BASE_URL}/api/notes?tag=${encodeURIComponent(tag)}`
  );
  if (!res.ok) {
    throw new Error(
      await parseError(res, `Failed to load notes (${res.status})`)
    );
  }
  return res.json();
}

export async function createNote({ title, text, tag }) {
  const res = await fetch(`${BASE_URL}/api/notes`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ title, text, tag }),
  });
  if (!res.ok) {
    throw new Error(
      await parseError(res, `Failed to create note (${res.status})`)
    );
  }
  return res.json();
}

export async function deleteNote(id) {
  const res = await fetch(`${BASE_URL}/api/notes/${id}`, {
    method: "DELETE",
  });
  if (!res.ok) {
    throw new Error(
      await parseError(res, `Failed to delete note (${res.status})`)
    );
  }
}
