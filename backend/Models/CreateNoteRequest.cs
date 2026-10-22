namespace NotesApi.Models;

/// <summary>
/// Payload accepted by POST /api/notes. Validation happens in the controller so we
/// can produce a clear 400 error for null/whitespace titles and tags.
/// </summary>
public class CreateNoteRequest
{
    public string? Title { get; set; }
    public string? Text { get; set; }
    public string? Tag { get; set; }
}
