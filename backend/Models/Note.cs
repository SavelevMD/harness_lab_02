namespace NotesApi.Models;

/// <summary>
/// A note. Serialized to JSON as { "id", "title", "text", "tag" }.
/// </summary>
public class Note
{
    /// <summary>Auto-incremented id, assigned by the store starting from 1.</summary>
    public int Id { get; set; }

    /// <summary>Required title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional body text (may be null or empty).</summary>
    public string? Text { get; set; }

    /// <summary>Required tag (single string, e.g. "work").</summary>
    public string Tag { get; set; } = string.Empty;
}
