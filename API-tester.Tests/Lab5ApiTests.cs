using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using API_tester.Data;
using API_tester.Dtos;
using API_tester.Models;
using API_tester.Models.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace API_tester.Tests;

public class Lab5ApiTests : IClassFixture<Lab5ApiFactory>
{
    private readonly Lab5ApiFactory _factory;

    public Lab5ApiTests(Lab5ApiFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/api/workspaces")]
    [InlineData("/api/collections")]
    [InlineData("/api/requests")]
    [InlineData("/api/request-headers")]
    [InlineData("/api/headers")]
    [InlineData("/api/environments")]
    [InlineData("/api/environment-variables")]
    [InlineData("/api/tags")]
    [InlineData("/api/request-attachments")]
    public async Task GetAll_ReturnsSuccess(string url)
    {
        var client = _factory.CreateClient();
        var suffix = url == "/api/request-attachments" ? "?requestId=1&q=seed" : "?q=seed";

        var response = await client.GetAsync(url + suffix);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Workspaces_CrudAndMissingIds_WorkForAdmin()
    {
        var client = _factory.CreateClient();
        var create = await client.PostAsJsonAsync("/api/workspaces", new WorkspaceWriteDto("Workspace CRUD", "test"));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<WorkspaceDto>();
        Assert.NotNull(created);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/workspaces/{created!.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/workspaces/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/workspaces/{created.Id}", new WorkspaceWriteDto("Workspace CRUD updated", "updated"))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync("/api/workspaces/999999", new WorkspaceWriteDto("Missing", null))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/workspaces", new WorkspaceWriteDto("", null))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/workspaces/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/workspaces/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task Collections_CrudAndMissingIds_WorkForAdmin()
    {
        var client = _factory.CreateClient();
        var workspaceId = await _factory.CreateWorkspaceAsync();
        var dto = new CollectionWriteDto("Collection CRUD", "test", true, workspaceId);
        var create = await client.PostAsJsonAsync("/api/collections", dto);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<CollectionDto>();
        Assert.NotNull(created);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/collections/{created!.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/collections/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/collections/{created.Id}", dto with { Name = "Collection updated" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync("/api/collections/999999", dto)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/collections", dto with { WorkspaceId = 0 })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/collections/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/collections/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task Requests_CrudAndMissingIds_WorkForAdmin()
    {
        var client = _factory.CreateClient();
        var collectionId = await _factory.CreateCollectionAsync();
        var tagId = await _factory.CreateTagAsync();
        var environmentId = await _factory.CreateEnvironmentAsync();
        var dto = new RequestWriteDto("Request CRUD", "https://example.com/api", HttpMethodType.Post, string.Empty, collectionId, environmentId, new[] { tagId });

        var create = await client.PostAsJsonAsync("/api/requests", dto);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<RequestDto>();
        Assert.NotNull(created);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/requests/{created!.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/requests/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/requests/{created.Id}", dto with { Name = "Request updated" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync("/api/requests/999999", dto)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/requests", dto with { CollectionId = 0 })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/requests/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/requests/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task Headers_CrudAndMissingIds_WorkForAdmin()
    {
        var client = _factory.CreateClient();
        var requestId = await _factory.CreateRequestAsync();
        var dto = new HeaderWriteDto(requestId, "X-Test", "yes", true);

        var create = await client.PostAsJsonAsync("/api/request-headers", dto);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<HeaderDto>();
        Assert.NotNull(created);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/request-headers/{created!.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/request-headers/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/request-headers/{created.Id}", dto with { Value = "updated" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync("/api/request-headers/999999", dto)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/request-headers", dto with { RequestId = 0 })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/request-headers/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/request-headers/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task Environments_CrudAndMissingIds_WorkForAdmin()
    {
        var client = _factory.CreateClient();
        var workspaceId = await _factory.CreateWorkspaceAsync();
        var dto = new EnvironmentWriteDto("Env CRUD", "https://example.com", EnvironmentType.Development, true, workspaceId);

        var create = await client.PostAsJsonAsync("/api/environments", dto);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<EnvironmentDto>();
        Assert.NotNull(created);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/environments/{created!.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/environments/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/environments/{created.Id}", dto with { Name = "Env updated" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync("/api/environments/999999", dto)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/environments", dto with { WorkspaceId = 0 })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/environments/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/environments/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task EnvironmentVariables_CrudAndMissingIds_WorkForAdmin()
    {
        var client = _factory.CreateClient();
        var environmentId = await _factory.CreateEnvironmentAsync();
        var dto = new EnvironmentVariableWriteDto(environmentId, "TOKEN", "abc", true, DateTime.UtcNow);

        var create = await client.PostAsJsonAsync("/api/environment-variables", dto);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<EnvironmentVariableDto>();
        Assert.NotNull(created);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/environment-variables/{created!.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/environment-variables/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/environment-variables/{created.Id}", dto with { Value = "updated" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync("/api/environment-variables/999999", dto)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/environment-variables", dto with { EnvironmentId = 0 })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/environment-variables/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/environment-variables/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task Tags_CrudAndMissingIds_WorkForAdmin()
    {
        var client = _factory.CreateClient();
        var dto = new TagWriteDto("tag-crud", "#13D0D4");

        var create = await client.PostAsJsonAsync("/api/tags", dto);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<TagDto>();
        Assert.NotNull(created);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/tags/{created!.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/tags/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/tags/{created.Id}", dto with { Name = "tag-updated" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync("/api/tags/999999", dto)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/tags", dto with { ColorHex = "bad" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/tags/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/tags/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task Attachments_CrudAndMissingIds_WorkForAdmin()
    {
        var client = _factory.CreateClient();
        var requestId = await _factory.CreateRequestAsync();

        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent("hello from test"u8.ToArray()), "file", "note.txt");

        var upload = await client.PostAsync($"/api/request-attachments/{requestId}", content);
        Assert.Equal(HttpStatusCode.Created, upload.StatusCode);
        var attachment = await upload.Content.ReadFromJsonAsync<AttachmentDto>();
        Assert.NotNull(attachment);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/request-attachments/{attachment!.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/request-attachments/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/request-attachments/{attachment.Id}", new AttachmentWriteDto("renamed.txt"))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync("/api/request-attachments/999999", new AttachmentWriteDto("missing.txt"))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync($"/api/request-attachments/{attachment.Id}", new AttachmentWriteDto(""))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/request-attachments/{attachment.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/request-attachments/{attachment.Id}")).StatusCode);
    }

    [Fact]
    public async Task AuthorizationRules_AreAppliedToApiMutations()
    {
        using var anonymousFactory = Lab5ApiFactory.CreateUnauthenticated();
        var anonymousClient = anonymousFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymousClient.PostAsJsonAsync("/api/tags", new TagWriteDto("blocked", "#13D0D4"))).StatusCode);

        using var managerFactory = Lab5ApiFactory.CreateWithRoles("Manager");
        var managerClient = managerFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var managerCreate = await managerClient.PostAsJsonAsync("/api/tags", new TagWriteDto("manager-tag", "#13D0D4"));
        Assert.Equal(HttpStatusCode.Created, managerCreate.StatusCode);
        var managerTag = await managerCreate.Content.ReadFromJsonAsync<TagDto>();
        Assert.NotNull(managerTag);
        Assert.Equal(HttpStatusCode.Forbidden, (await managerClient.DeleteAsync($"/api/tags/{managerTag!.Id}")).StatusCode);
    }
}

public class Lab5ApiFactory : WebApplicationFactory<Program>
{
    private readonly bool _authenticated;
    private readonly bool _useTestAuthentication;
    private readonly string[] _roles;
    private readonly string _databaseName = $"api_tester_tests_{Guid.NewGuid():N}";
    private readonly string _webRoot = Path.Combine(Path.GetTempPath(), "api-tester-tests", Guid.NewGuid().ToString("N"), "wwwroot");

    public Lab5ApiFactory()
        : this(true, true, new[] { "Admin", "Manager" })
    {
    }

    public static Lab5ApiFactory CreateUnauthenticated() => new(false, true, Array.Empty<string>());
    public static Lab5ApiFactory CreateWithApplicationAuthentication() => new(false, false, Array.Empty<string>());
    public static Lab5ApiFactory CreateWithRoles(params string[] roles) => new(true, true, roles);

    private Lab5ApiFactory(bool authenticated, bool useTestAuthentication, params string[] roles)
    {
        _authenticated = authenticated;
        _useTestAuthentication = useTestAuthentication;
        _roles = roles;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Directory.CreateDirectory(_webRoot);
        builder.UseWebRoot(_webRoot);
        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureTestServices(services =>
        {
            var dbOptions = services.SingleOrDefault(descriptor => descriptor.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbOptions != null)
            {
                services.Remove(dbOptions);
            }

            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));

            if (_useTestAuthentication)
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.AuthScheme;
                    options.DefaultChallengeScheme = TestAuthHandler.AuthScheme;
                    options.DefaultForbidScheme = TestAuthHandler.AuthScheme;
                })
                .AddScheme<TestAuthOptions, TestAuthHandler>(TestAuthHandler.AuthScheme, options =>
                {
                    options.Authenticated = _authenticated;
                    options.Roles = _roles;
                });
            }

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            Seed(db);
        });
    }

    public async Task<int> CreateWorkspaceAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var workspace = new ApiWorkspace { Name = $"Workspace {Guid.NewGuid():N}", Description = "Test", CreatedAt = DateTime.UtcNow };
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();
        return workspace.Id;
    }

    public async Task<int> CreateCollectionAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var workspace = new ApiWorkspace { Name = $"Workspace {Guid.NewGuid():N}", Description = "Test", CreatedAt = DateTime.UtcNow };
        var collection = new ApiCollection { Name = $"Collection {Guid.NewGuid():N}", Description = "Test", IsShared = true, Workspace = workspace, CreatedAt = DateTime.UtcNow };
        db.Collections.Add(collection);
        await db.SaveChangesAsync();
        return collection.Id;
    }

    public async Task<int> CreateRequestAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var workspace = new ApiWorkspace { Name = $"Workspace {Guid.NewGuid():N}", Description = "Test", CreatedAt = DateTime.UtcNow };
        var collection = new ApiCollection { Name = $"Collection {Guid.NewGuid():N}", Description = "Test", Workspace = workspace, CreatedAt = DateTime.UtcNow };
        var request = new ApiRequest { Name = $"Request {Guid.NewGuid():N}", Url = "https://example.com/api", Method = HttpMethodType.Get, Body = string.Empty, Collection = collection, CreatedAt = DateTime.UtcNow };
        db.Requests.Add(request);
        await db.SaveChangesAsync();
        return request.Id;
    }

    public async Task<int> CreateEnvironmentAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var workspace = new ApiWorkspace { Name = $"Workspace {Guid.NewGuid():N}", Description = "Test", CreatedAt = DateTime.UtcNow };
        var environment = new ApiEnvironment { Name = $"Env {Guid.NewGuid():N}", BaseUrl = "https://example.com", Type = EnvironmentType.Development, IsActive = true, Workspace = workspace, CreatedAt = DateTime.UtcNow };
        db.Environments.Add(environment);
        await db.SaveChangesAsync();
        return environment.Id;
    }

    public async Task<int> CreateTagAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tag = new RequestTag { Name = $"tag-{Guid.NewGuid():N}", ColorHex = "#13D0D4", CreatedAt = DateTime.UtcNow };
        db.RequestTags.Add(tag);
        await db.SaveChangesAsync();
        return tag.Id;
    }

    private static void Seed(AppDbContext db)
    {
        if (db.Workspaces.Any())
        {
            return;
        }

        db.Roles.AddRange(
            new IdentityRole<int> { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole<int> { Id = 2, Name = "Manager", NormalizedName = "MANAGER" });

        var workspace = new ApiWorkspace { Name = "Seed workspace", Description = "Seeded", CreatedAt = DateTime.UtcNow };
        var collection = new ApiCollection { Name = "Seed collection", Description = "Seeded", Workspace = workspace, CreatedAt = DateTime.UtcNow };
        var request = new ApiRequest { Name = "Seed request", Url = "https://example.com/api", Method = HttpMethodType.Get, Body = string.Empty, Collection = collection, CreatedAt = DateTime.UtcNow };
        var environment = new ApiEnvironment { Name = "Seed environment", BaseUrl = "https://example.com", Type = EnvironmentType.Development, IsActive = true, Workspace = workspace, CreatedAt = DateTime.UtcNow };
        var variable = new EnvironmentVariable { Environment = environment, Key = "SEED", Value = "value", IsSecret = false, LastUpdatedAt = DateTime.UtcNow };
        var tag = new RequestTag { Name = "seed", ColorHex = "#13D0D4", CreatedAt = DateTime.UtcNow };
        var header = new ApiHeader { Request = request, Key = "X-Seed", Value = "true", IsEnabled = true };
        var attachment = new RequestAttachment { Request = request, FileName = "seed.txt", StoredFileName = "seed.txt", FilePath = "/uploads/requests/1/seed.txt", ContentType = "text/plain", FileSize = 4, CreatedAt = DateTime.UtcNow };

        db.Workspaces.Add(workspace);
        db.Collections.Add(collection);
        db.Requests.Add(request);
        db.Environments.Add(environment);
        db.EnvironmentVariables.Add(variable);
        db.RequestTags.Add(tag);
        db.Headers.Add(header);
        db.RequestAttachments.Add(attachment);
        db.SaveChanges();
    }
}

public class TestAuthOptions : AuthenticationSchemeOptions
{
    public bool Authenticated { get; set; } = true;
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
}

public class TestAuthHandler : AuthenticationHandler<TestAuthOptions>
{
    public const string AuthScheme = "Test";

    public TestAuthHandler(IOptionsMonitor<TestAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Options.Authenticated)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "test-admin@example.com")
        };
        claims.AddRange(Options.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, AuthScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthScheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
