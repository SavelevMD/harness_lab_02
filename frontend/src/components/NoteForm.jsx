import { useState } from "react";

export default function NoteForm({ onSubmit, submitting }) {
  const [title, setTitle] = useState("");
  const [text, setText] = useState("");
  const [tag, setTag] = useState("");
  const [touched, setTouched] = useState({ title: false, tag: false });

  const titleInvalid = title.trim() === "";
  const tagInvalid = tag.trim() === "";
  const canSubmit = !titleInvalid && !tagInvalid && !submitting;

  function handleSubmit(e) {
    e.preventDefault();
    setTouched({ title: true, tag: true });
    if (titleInvalid || tagInvalid) return;
    onSubmit({ title: title.trim(), text: text.trim(), tag: tag.trim() });
    setTitle("");
    setText("");
    setTag("");
    setTouched({ title: false, tag: false });
  }

  return (
    <form onSubmit={handleSubmit} noValidate aria-label="New note form">
      <div className="field">
        <label htmlFor="title">Title</label>
        <input
          id="title"
          value={title}
          placeholder="Note title"
          aria-required="true"
          onChange={(e) => setTitle(e.target.value)}
          onBlur={() => setTouched((t) => ({ ...t, title: true }))}
        />
        {touched.title && titleInvalid && (
          <span className="field-error" role="alert">
            Title is required
          </span>
        )}
      </div>

      <div className="field">
        <label htmlFor="text">Text</label>
        <textarea
          id="text"
          value={text}
          placeholder="Note text (optional)"
          onChange={(e) => setText(e.target.value)}
        />
      </div>

      <div className="field">
        <label htmlFor="tag">Tag</label>
        <input
          id="tag"
          value={tag}
          placeholder="e.g. work, personal, ideas"
          aria-required="true"
          onChange={(e) => setTag(e.target.value)}
          onBlur={() => setTouched((t) => ({ ...t, tag: true }))}
        />
        {touched.tag && tagInvalid && (
          <span className="field-error" role="alert">
            Tag is required
          </span>
        )}
      </div>

      <button type="submit" disabled={!canSubmit}>
        {submitting ? "Adding…" : "Add note"}
      </button>
    </form>
  );
}
