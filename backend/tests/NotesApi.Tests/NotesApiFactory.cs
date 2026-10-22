using Microsoft.AspNetCore.Mvc.Testing;

namespace NotesApi.Tests;

/// <summary>
/// Host factory for the Notes API. A single factory (and therefore a single
/// in-memory store singleton) is shared across all tests in the collection;
/// each test clears the store in its setup to stay isolated.
/// </summary>
public sealed class NotesApiFactory : WebApplicationFactory<Program>
{
}
