using NotesApi.Models;

namespace NotesApi.Storage;

/// <summary>Contract for the in-memory note storage.</summary>
public interface INoteStore
{
    /// <summary>Adds a note, assigning it a new unique id, and returns the stored note.</summary>
    Note Add(Note note);

    /// <summary>Returns all notes, ordered by id ascending.</summary>
    IReadOnlyList<Note> GetAll();

    /// <summary>Returns notes whose tag matches exactly (case-insensitive), ordered by id.</summary>
    IReadOnlyList<Note> GetByTag(string tag);

    /// <summary>Returns the note with the given id, or null if it does not exist.</summary>
    Note? GetById(int id);

    /// <summary>Deletes the note with the given id. Returns true if one was removed.</summary>
    bool TryDelete(int id, out Note? note);

    /// <summary>Removes every note and resets the id counter (used by tests).</summary>
    void Clear();
}
