export default function NoteList({ notes, loading, onDelete }) {
  if (loading) {
    return (
      <p className="status" role="status">
        Loading notes…
      </p>
    );
  }

  if (notes.length === 0) {
    return <p className="status">Нет записей</p>;
  }

  return (
    <ul className="note-list">
      {notes.map((note) => (
        <li key={note.id} className="note-item" data-testid={`note-${note.id}`}>
          <div className="note-content">
            <h3>{note.title}</h3>
            {note.text && <p>{note.text}</p>}
            <span className="note-tag">{note.tag}</span>
          </div>
          <button
            type="button"
            onClick={() => onDelete(note.id)}
            aria-label={`Delete note ${note.title}`}
          >
            Delete
          </button>
        </li>
      ))}
    </ul>
  );
}
