using System.Text.Json;
using NotesApi.Storage;

var builder = WebApplication.CreateBuilder(args);

// Register controllers with camelCase JSON serialization so the contract's
// `title`, `text` and `tag` fields stay lowercase in responses.
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// In-memory store (prototype: no persistence).
builder.Services.AddSingleton<INoteStore, InMemoryNoteStore>();

// CORS: allow the Vite dev server (and a fallback port).
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("AllowFrontend");
app.MapControllers();

app.Run();

// Exposed so the integration tests can reference it via WebApplicationFactory<Program>.
public partial class Program
{
}
