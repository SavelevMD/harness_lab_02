using Microsoft.AspNetCore.Mvc;
using NotesApi.Models;
using NotesApi.Storage;

namespace NotesApi.Controllers;

[ApiController]
[Route("api/notes")]
public class NotesController : ControllerBase
{
    private readonly INoteStore _store;

    public NotesController(INoteStore store)
    {
        _store = store;
    }

    /// <summary>POST /api/notes — creates a note and returns 201 with the assigned id.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Note), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Note> Create([FromBody] CreateNoteRequest? request)
    {
        if (request is null)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation error",
                detail: "A JSON request body is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation error",
                detail: "Title is required and cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(request.Tag))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation error",
                detail: "Tag is required and cannot be empty or whitespace.");
        }

        var note = new Note
        {
            Title = request.Title,
            Text = request.Text,
            Tag = request.Tag
        };

        note = _store.Add(note);

        return CreatedAtAction(nameof(GetById), new { id = note.Id }, note);
    }

    /// <summary>GET /api/notes — returns all notes; pass ?tag=work to filter (case-insensitive).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Note>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<Note>> GetAll([FromQuery] string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return Ok(_store.GetAll());
        }

        return Ok(_store.GetByTag(tag));
    }

    /// <summary>GET /api/notes/{id} — returns a single note or 404.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Note), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Note> GetById(int id)
    {
        var note = _store.GetById(id);
        return note is null ? NotFound() : Ok(note);
    }

    /// <summary>DELETE /api/notes/{id} — removes the note; 404 if it does not exist.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        return _store.TryDelete(id, out _) ? NoContent() : NotFound();
    }
}
