using System.Collections.Concurrent;
using NotesApi.Models;

namespace NotesApi.Storage;

/// <summary>
/// Thread-safe in-memory note store. Ids are stable, unique and auto-incremented
/// starting from 1. No persistence by design (prototype).
/// </summary>
public sealed class InMemoryNoteStore : INoteStore
{
    private readonly ConcurrentDictionary<int, Note> _notes = new();
    private int _nextId;

    public Note Add(Note note)
    {
        // Interlocked.Increment returns the incremented value, so the first call yields 1.
        var id = Interlocked.Increment(ref _nextId);
        note.Id = id;
        _notes[id] = note;
        return note;
    }

    public IReadOnlyList<Note> GetAll() =>
        _notes.Values.OrderBy(n => n.Id).ToArray();

    public IReadOnlyList<Note> GetByTag(string tag) =>
        _notes.Values
            .Where(n => string.Equals(n.Tag, tag, StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n.Id)
            .ToArray();

    public Note? GetById(int id) =>
        _notes.TryGetValue(id, out var note) ? note : null;

    public bool TryDelete(int id, out Note? note) =>
        _notes.TryRemove(id, out note);

    public void Clear()
    {
        _notes.Clear();
        _nextId = 0;
    }
}
