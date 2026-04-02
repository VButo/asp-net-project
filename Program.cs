using API_tester.Models;
using API_tester.Models.Enums;

if (args.Contains("--web", StringComparer.OrdinalIgnoreCase))
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
    return;
}

await RunLabDemoAsync();

static async Task RunLabDemoAsync()
{
    var owner = new User
    {
        Id = Guid.NewGuid(),
        Username = "ana.dev",
        Email = "ana@apitester.dev",
        Role = "Owner",
        IsActive = true,
        CreatedAt = DateTime.UtcNow.AddMonths(-2)
    };

    var workspace = new ApiWorkspace
    {
        Id = Guid.NewGuid(),
        Name = "API Tester Workspace",
        Description = "Workspace for testing REST endpoints",
        CreatedAt = DateTime.UtcNow.AddDays(-45),
        OwnerUserId = owner.Id,
        OwnerUser = owner
    };

    var devEnv = new ApiEnvironment
    {
        Id = Guid.NewGuid(),
        Name = "Development",
        Type = EnvironmentType.Development,
        BaseUrl = "https://dev.api.local",
        IsActive = true,
        CreatedAt = DateTime.UtcNow.AddDays(-40),
        WorkspaceId = workspace.Id,
        Workspace = workspace
    };

    var stagingEnv = new ApiEnvironment
    {
        Id = Guid.NewGuid(),
        Name = "Staging",
        Type = EnvironmentType.Staging,
        BaseUrl = "https://staging.api.local",
        IsActive = true,
        CreatedAt = DateTime.UtcNow.AddDays(-35),
        WorkspaceId = workspace.Id,
        Workspace = workspace
    };

    workspace.Environments.Add(devEnv);
    workspace.Environments.Add(stagingEnv);

    devEnv.Variables.Add(new EnvironmentVariable
    {
        Id = Guid.NewGuid(),
        Environment = devEnv,
        EnvironmentId = devEnv.Id,
        Key = "token",
        Value = "dev-token-123",
        IsSecret = true,
        LastUpdatedAt = DateTime.UtcNow.AddDays(-2)
    });

    var tagAuth = new RequestTag
    {
        Id = Guid.NewGuid(),
        Name = "Auth",
        ColorHex = "#2E7D32",
        CreatedAt = DateTime.UtcNow.AddDays(-20)
    };

    var tagUsers = new RequestTag
    {
        Id = Guid.NewGuid(),
        Name = "Users",
        ColorHex = "#1565C0",
        CreatedAt = DateTime.UtcNow.AddDays(-20)
    };

    var collections = new List<ApiCollection>
    {
        new ApiCollection
        {
            Id = Guid.NewGuid(),
            Name = "Auth API",
            Description = "Login and token refresh endpoints",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            IsShared = true,
            WorkspaceId = workspace.Id,
            Workspace = workspace
        },
        new ApiCollection
        {
            Id = Guid.NewGuid(),
            Name = "Users API",
            Description = "User management endpoints",
            CreatedAt = DateTime.UtcNow.AddDays(-25),
            IsShared = true,
            WorkspaceId = workspace.Id,
            Workspace = workspace
        },
        new ApiCollection
        {
            Id = Guid.NewGuid(),
            Name = "Orders API",
            Description = "Order processing endpoints",
            CreatedAt = DateTime.UtcNow.AddDays(-15),
            IsShared = false,
            WorkspaceId = workspace.Id,
            Workspace = workspace
        }
    };

    workspace.Collections.AddRange(collections);

    AddRequest(collections[0], "Login", "/auth/login", HttpMethodType.Post, "{\"email\":\"ana@apitester.dev\",\"password\":\"demo\"}", tagAuth, devEnv, stagingEnv);
    AddRequest(collections[0], "Refresh token", "/auth/refresh", HttpMethodType.Post, "{\"refreshToken\":\"abc\"}", tagAuth, devEnv);
    AddRequest(collections[1], "Get users", "/users", HttpMethodType.Get, string.Empty, tagUsers, devEnv, stagingEnv);
    AddRequest(collections[1], "Create user", "/users", HttpMethodType.Post, "{\"name\":\"Luka\"}", tagUsers, devEnv);
    AddRequest(collections[2], "Get order", "/orders/42", HttpMethodType.Get, string.Empty, tagUsers, stagingEnv);
    AddRequest(collections[2], "Delete order", "/orders/42", HttpMethodType.Delete, string.Empty, tagUsers, stagingEnv);

    var allRequests = collections.SelectMany(c => c.Requests).ToList();
    var executor = new ApiRequestExecutor();

    await Task.WhenAll(allRequests.Select(r => executor.ExecuteRequestAsync(r)));

    Console.WriteLine("=== LINQ QUERIES ===");

    var getRequests = allRequests
        .Where(r => r.Method == HttpMethodType.Get)
        .OrderBy(r => r.Name)
        .ToList();
    Console.WriteLine($"GET requests: {getRequests.Count}");

    var latestExecutedRequest = allRequests
        .Where(r => r.LastExecutedAt.HasValue)
        .OrderByDescending(r => r.LastExecutedAt)
        .FirstOrDefault();
    Console.WriteLine($"Latest executed request: {latestExecutedRequest?.Name}");

    var requestsPerCollection = collections
        .Select(c => new { c.Name, RequestCount = c.Requests.Count })
        .OrderByDescending(x => x.RequestCount)
        .ToList();
    foreach (var item in requestsPerCollection)
    {
        Console.WriteLine($"Collection '{item.Name}' has {item.RequestCount} requests");
    }

    var failedResponses = allRequests
        .SelectMany(r => r.Responses)
        .Where(resp => !resp.IsSuccess)
        .OrderByDescending(resp => resp.ReceivedAt)
        .ToList();
    Console.WriteLine($"Failed responses: {failedResponses.Count}");

    var requestsSortedByCreation = allRequests
        .OrderBy(r => r.CreatedAt)
        .Select(r => new { r.Name, r.CreatedAt })
        .ToList();
    Console.WriteLine("Requests sorted by creation date:");
    foreach (var item in requestsSortedByCreation)
    {
        Console.WriteLine($"- {item.Name} ({item.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC)");
    }

    var requestsLinkedToMultipleEnvironments = allRequests
        .Where(r => r.EnvironmentLinks.Count > 1)
        .Select(r => r.Name)
        .ToList();
    Console.WriteLine($"Requests linked to multiple environments: {requestsLinkedToMultipleEnvironments.Count}");
}

static void AddRequest(
    ApiCollection collection,
    string name,
    string path,
    HttpMethodType method,
    string body,
    RequestTag tag,
    params ApiEnvironment[] environments)
{
    var request = new ApiRequest
    {
        Id = Guid.NewGuid(),
        Name = name,
        Url = path,
        Method = method,
        Body = body,
        CreatedAt = DateTime.UtcNow.AddDays(-new Random().Next(1, 28)),
        CollectionId = collection.Id,
        Collection = collection
    };

    request.Headers.Add(new ApiHeader
    {
        Id = Guid.NewGuid(),
        RequestId = request.Id,
        Request = request,
        Key = "Accept",
        Value = "application/json",
        IsEnabled = true
    });

    foreach (var env in environments)
    {
        var envLink = new RequestEnvironmentLink
        {
            RequestId = request.Id,
            Request = request,
            EnvironmentId = env.Id,
            Environment = env,
            LinkedAt = DateTime.UtcNow,
            IsDefaultEnvironment = env.Type == EnvironmentType.Development
        };

        request.EnvironmentLinks.Add(envLink);
        env.RequestLinks.Add(envLink);
    }

    var tagLink = new RequestTagMap
    {
        RequestId = request.Id,
        Request = request,
        TagId = tag.Id,
        Tag = tag,
        LinkedAt = DateTime.UtcNow
    };

    request.TagLinks.Add(tagLink);
    tag.RequestLinks.Add(tagLink);

    collection.Requests.Add(request);
}
