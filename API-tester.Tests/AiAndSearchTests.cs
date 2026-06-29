using System.Net;
using System.Net.Http.Json;
using API_tester.Models;
using Xunit;

namespace API_tester.Tests;

public class AiAndSearchTests
{
    [Fact]
    public async Task AiDraft_UsesLocalFallbackWithoutApiKey()
    {
        using var factory = Lab5ApiFactory.CreateWithRoles("Manager");
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/ai/request-draft",
            new AiRequestDraftInput("Create a GET request to fetch users from JSONPlaceholder"));
        var draft = await response.Content.ReadFromJsonAsync<AiRequestDraft>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(draft);
        Assert.Equal("local", draft!.Source);
        Assert.Equal("https://jsonplaceholder.typicode.com/users", draft.Url);
    }

    [Fact]
    public async Task AiDraft_RejectsEmptyPrompt()
    {
        using var factory = Lab5ApiFactory.CreateWithRoles("Manager");
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/ai/request-draft", new AiRequestDraftInput(""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GlobalSearch_ReturnsLinkedSeededResults()
    {
        using var factory = Lab5ApiFactory.CreateWithRoles("Admin");
        var client = factory.CreateClient();

        var response = await client.GetAsync("/search?q=seed");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Seed workspace", html);
        Assert.Contains("/request-builder/1", html);
    }
}
