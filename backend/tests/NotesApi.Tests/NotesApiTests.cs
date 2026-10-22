using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using NotesApi.Models;
using NotesApi.Storage;

namespace NotesApi.Tests;

/// <summary>
/// Integration tests covering every acceptance criterion of the "Notes with tags" feature.
/// </summary>
public class NotesApiTests : IClassFixture<NotesApiFactory>
{
    private readonly HttpClient _client;
    private readonly INoteStore _store;

    public NotesApiTests(NotesApiFactory factory)
    {
        _client = factory.CreateClient();
        _store = factory.Services.GetRequiredService<INoteStore>();
        _store.Clear();
    }

    // ---------- helpers ----------

    private async Task<HttpResponseMessage> PostJsonAsync(string url, string json)
    {
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _client.PostAsync(url, content);
    }

    private async Task<Note> CreateNoteAsync(string title, string? text, string tag)
    {
        var response = await _client.PostAsJsonAsync("/api/notes", new { title, text, tag });
        response.EnsureSuccessStatusCode();
        var note = await response.Content.ReadFromJsonAsync<Note>();
        Assert.NotNull(note);
        return note!;
    }

    // ---------- POST /api/notes ----------

    [Fact]
    public async Task Create_Returns201_WithAssignedId_AndEchoesFields()
    {
        var response = await _client.PostAsJsonAsync("/api/notes",
            new { title = "First note", text = "hello world", tag = "work" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var raw = await response.Content.ReadAsStringAsync();
        // JSON contract must be camelCase: id/title/text/tag
        Assert.Contains("\"id\"", raw);
        Assert.Contains("\"title\"", raw);
        Assert.Contains("\"text\"", raw);
        Assert.Contains("\"tag\"", raw);
        Assert.DoesNotContain("\"Title\"", raw);

        var note = await response.Content.ReadFromJsonAsync<Note>();
        Assert.Equal(1, note!.Id);
        Assert.Equal("First note", note.Title);
        Assert.Equal("hello world", note.Text);
        Assert.Equal("work", note.Tag);

        // Location header should point at the created resource.
        Assert.NotNull(response.Headers.Location);
        Assert.EndsWith("/api/notes/1", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Create_AssignsIncrementingIds()
    {
        var first = await CreateNoteAsync("One", null, "work");
        var second = await CreateNoteAsync("Two", null, "personal");
        var third = await CreateNoteAsync("Three", null, "ideas");

        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
        Assert.Equal(3, third.Id);
    }

    [Fact]
    public async Task Create_TextIsOptional_WhenOmitted_TextIsNull()
    {
        // Explicit JSON without a "text" field.
        var response = await PostJsonAsync("/api/notes", """{ "title": "No text", "tag": "work" }""");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var note = await response.Content.ReadFromJsonAsync<Note>();
        Assert.NotNull(note);
        Assert.Null(note!.Text);
    }

    [Fact]
    public async Task Create_TextIsOptional_WhenNull_TextIsNull()
    {
        var response = await PostJsonAsync("/api/notes", """{ "title": "Null text", "text": null, "tag": "work" }""");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var note = await response.Content.ReadFromJsonAsync<Note>();
        Assert.NotNull(note);
        Assert.Null(note!.Text);
    }

    [Theory]
    [InlineData("""{ "title": "", "tag": "work" }""")]
    [InlineData("""{ "title": "   ", "tag": "work" }""")]
    [InlineData("""{ "text": "no title", "tag": "work" }""")]
    public async Task Create_InvalidTitle_Returns400(string json)
    {
        var response = await PostJsonAsync("/api/notes", json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("""{ "title": "valid", "tag": "" }""")]
    [InlineData("""{ "title": "valid", "tag": "   " }""")]
    [InlineData("""{ "title": "valid" }""")]
    public async Task Create_InvalidTag_Returns400(string json)
    {
        var response = await PostJsonAsync("/api/notes", json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Create_MissingBody_Returns400()
    {
        var response = await _client.PostAsync("/api/notes", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- GET /api/notes ----------

    [Fact]
    public async Task GetAll_WhenEmpty_ReturnsEmptyArray()
    {
        var response = await _client.GetAsync("/api/notes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var notes = await response.Content.ReadFromJsonAsync<List<Note>>();
        Assert.NotNull(notes);
        Assert.Empty(notes!);
    }

    [Fact]
    public async Task GetAll_ReturnsAllNotes_InIdOrder()
    {
        await CreateNoteAsync("A", null, "work");
        await CreateNoteAsync("B", null, "personal");
        await CreateNoteAsync("C", null, "ideas");

        var response = await _client.GetAsync("/api/notes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var notes = await response.Content.ReadFromJsonAsync<List<Note>>();
        Assert.NotNull(notes);
        Assert.Equal(3, notes!.Count);
        Assert.Equal(new[] { 1, 2, 3 }, notes.Select(n => n.Id).ToArray());
    }

    // ---------- GET /api/notes?tag=... ----------

    [Fact]
    public async Task GetByTag_FiltersByExactCaseInsensitiveTag()
    {
        await CreateNoteAsync("work 1", null, "work");
        await CreateNoteAsync("work 2", null, "Work");
        await CreateNoteAsync("personal 1", null, "personal");
        await CreateNoteAsync("personal 2", null, "personal");

        var response = await _client.GetAsync("/api/notes?tag=work");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var notes = await response.Content.ReadFromJsonAsync<List<Note>>();
        Assert.NotNull(notes);
        Assert.Equal(2, notes!.Count);
        Assert.All(notes, n => Assert.Equal("work", n.Tag, ignoreCase: true));
    }

    [Fact]
    public async Task GetByTag_IsCaseInsensitive_OnTheQueryValue()
    {
        await CreateNoteAsync("work 1", null, "work");
        await CreateNoteAsync("personal 1", null, "personal");

        var response = await _client.GetAsync("/api/notes?tag=WORK");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var notes = await response.Content.ReadFromJsonAsync<List<Note>>();
        Assert.Single(notes!);
        Assert.Equal("work", notes![0].Tag);
    }

    [Fact]
    public async Task GetByTag_NoMatches_ReturnsEmptyArray()
    {
        await CreateNoteAsync("work 1", null, "work");

        var response = await _client.GetAsync("/api/notes?tag=nonexistent");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var notes = await response.Content.ReadFromJsonAsync<List<Note>>();
        Assert.NotNull(notes);
        Assert.Empty(notes!);
    }

    [Fact]
    public async Task GetByTag_AbsentTagParam_ReturnsAllNotes()
    {
        await CreateNoteAsync("work 1", null, "work");
        await CreateNoteAsync("personal 1", null, "personal");

        var response = await _client.GetAsync("/api/notes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var notes = await response.Content.ReadFromJsonAsync<List<Note>>();
        Assert.Equal(2, notes!.Count);
    }

    // ---------- DELETE /api/notes/{id} ----------

    [Fact]
    public async Task Delete_Existing_Returns204_AndRemoves()
    {
        await CreateNoteAsync("to delete", null, "work");

        var response = await _client.DeleteAsync("/api/notes/1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var remaining = await _client.GetFromJsonAsync<List<Note>>("/api/notes");
        Assert.Empty(remaining!);
    }

    [Fact]
    public async Task Delete_RepeatedCall_Returns404()
    {
        await CreateNoteAsync("to delete", null, "work");

        var first = await _client.DeleteAsync("/api/notes/1");
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);

        var second = await _client.DeleteAsync("/api/notes/1");
        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
    }

    [Fact]
    public async Task Delete_NonexistentId_Returns404()
    {
        var response = await _client.DeleteAsync("/api/notes/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- GET /api/notes/{id} ----------

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        await CreateNoteAsync("single", "body", "work");

        var response = await _client.GetAsync("/api/notes/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var note = await response.Content.ReadFromJsonAsync<Note>();
        Assert.Equal(1, note!.Id);
        Assert.Equal("single", note.Title);
    }

    [Fact]
    public async Task GetById_Nonexistent_Returns404()
    {
        var response = await _client.GetAsync("/api/notes/404");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- CORS ----------

    [Theory]
    [InlineData("http://localhost:5173")]
    [InlineData("http://localhost:3000")]
    public async Task Cors_AllowsConfiguredOrigins(string origin)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/notes");
        request.Headers.Add("Origin", origin);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values));
        Assert.Contains(origin, values!);
    }
}
