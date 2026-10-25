import { useCallback, useEffect, useState } from "react";
import { getNotes, getNotesByTag, createNote, deleteNote } from "./api";
import NoteForm from "./components/NoteForm";
import TagFilter from "./components/TagFilter";
import NoteList from "./components/NoteList";
import Clock from "./components/Clock";

function distinctTags(notes) {
  return [...new Set(notes.map((n) => n.tag).filter(Boolean))];
}

export default function App() {
  const [notes, setNotes] = useState([]);
  const [allTags, setAllTags] = useState([]);
  const [filterTag, setFilterTag] = useState("All");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  const loadNotes = useCallback(async (tag) => {
    setLoading(true);
    setError(null);
    try {
      const all = await getNotes();
      setAllTags(distinctTags(all));
      if (tag === "All") {
        setNotes(all);
      } else {
        const filtered = await getNotesByTag(tag);
        setNotes(filtered);
      }
    } catch (e) {
      setError(e.message || "Failed to load notes");
      setNotes([]);
      setAllTags([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadNotes(filterTag);
  }, [filterTag, loadNotes]);

  async function handleCreate({ title, text, tag }) {
    setSubmitting(true);
    setError(null);
    try {
      await createNote({ title, text, tag });
      await loadNotes(filterTag);
    } catch (e) {
      setError(e.message || "Failed to create note");
    } finally {
      setSubmitting(false);
    }
  }

  async function handleDelete(id) {
    setError(null);
    try {
      await deleteNote(id);
      await loadNotes(filterTag);
    } catch (e) {
      setError(e.message || "Failed to delete note");
    }
  }

  return (
    <main className="app">
      <header className="app-header">
        <h1>Записки</h1>
        <Clock />
      </header>

      <NoteForm onSubmit={handleCreate} submitting={submitting} />

      <TagFilter tags={allTags} value={filterTag} onChange={setFilterTag} />

      {error && (
        <p className="error" role="alert">
          {error}
        </p>
      )}

      <NoteList notes={notes} loading={loading} onDelete={handleDelete} />
    </main>
  );
}
