using Microsoft.AspNetCore.Mvc;
using API_tester.Models;
using API_tester.Models.Enums;

namespace API_tester.Controllers;

public class RequestController : Controller
{
    public IActionResult Index()
    {
        var requests = BuildRequests();

        ViewData["Title"] = "Requests";
        ViewData["BreadcrumbCurrent"] = "Requests";
        ViewData["HeroKicker"] = "Request Matrix";
        ViewData["HeroTitle"] = "Operativni pregled svih API requestova kroz collectione i workspacove.";
        ViewData["HeroDescription"] = "Fokusiran prikaz method badgeova, URL-ova, pripadnih collectiona, tagova i zadnjeg izvrsenja ili statusa svake rute.";
        ViewData["PrimaryActionText"] = "+ New Request";
        ViewData["SecondaryActionText"] = "Run Batch";
        ViewData["SearchPlaceholder"] = "Search by URL, collection or tag";
        ViewData["Filters"] = new List<string> { "All Methods", "Critical", "Smoke", "Pending" };

        var withResponse = requests.Where(r => r.Responses.Any()).ToList();
        ViewData["SnapshotTitle"] = $"{requests.Count} Requests";
        ViewData["SuccessfulCount"] = withResponse.Count(r => r.Responses.OrderByDescending(resp => resp.ReceivedAt).First().IsSuccess);
        ViewData["WarningCount"] = withResponse.Count(r => !r.Responses.OrderByDescending(resp => resp.ReceivedAt).First().IsSuccess);
        ViewData["NotExecutedCount"] = requests.Count(r => !r.Responses.Any());

        return View(requests);
    }

    private static List<ApiRequest> BuildRequests()
    {
        var paymentsWorkspace = CreateWorkspace("Payments Core");
        var identityWorkspace = CreateWorkspace("Identity Gateway");
        var ordersWorkspace = CreateWorkspace("Orders Public API");
        var notificationsWorkspace = CreateWorkspace("Notifications Hub");
        var fraudWorkspace = CreateWorkspace("Fraud Engine");
        var sandboxWorkspace = CreateWorkspace("Developer Sandbox");

        var paymentsCollection = CreateCollection(paymentsWorkspace, "Payments Authorization");
        var identityCollection = CreateCollection(identityWorkspace, "Identity Session Flow");
        var ordersCollection = CreateCollection(ordersWorkspace, "Orders Partner API");
        var notificationsCollection = CreateCollection(notificationsWorkspace, "Notification Dispatch");
        var fraudCollection = CreateCollection(fraudWorkspace, "Fraud Rules and Scoring");
        var sandboxCollection = CreateCollection(sandboxWorkspace, "Sandbox Smoke Set");

        return new List<ApiRequest>
        {
            CreateRequest(paymentsCollection, "/v1/payments/authorize", HttpMethodType.Post, 200, true, 143, "critical", "pci", "checkout"),
            CreateRequest(identityCollection, "/v1/sessions/{sessionId}", HttpMethodType.Get, 200, true, 89, "auth", "mobile"),
            CreateRequest(ordersCollection, "/partner/v2/orders/{orderId}/status", HttpMethodType.Patch, 409, false, 310, "partner", "state-change"),
            CreateRequest(notificationsCollection, "/v1/notifications/dispatch", HttpMethodType.Post, 202, true, 121, "webhook", "retry", "email"),
            CreateRequest(fraudCollection, "/risk/v1/score/{transactionId}", HttpMethodType.Get, 200, true, 97, "risk", "fraud", "realtime"),
            CreateRequestWithoutExecution(sandboxCollection, "/sandbox/v1/sessions/{sessionId}", HttpMethodType.Delete, "sandbox", "cleanup")
        };
    }

    private static ApiWorkspace CreateWorkspace(string name)
    {
        return new ApiWorkspace
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"{name} workspace",
            CreatedAt = DateTime.UtcNow.AddDays(-40)
        };
    }

    private static ApiCollection CreateCollection(ApiWorkspace workspace, string collectionName)
    {
        var collection = new ApiCollection
        {
            Id = Guid.NewGuid(),
            Name = collectionName,
            Description = $"{collectionName} endpoints",
            CreatedAt = DateTime.UtcNow.AddDays(-20),
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            IsShared = true
        };

        workspace.Collections.Add(collection);
        return collection;
    }

    private static ApiRequest CreateRequest(
        ApiCollection collection,
        string url,
        HttpMethodType method,
        int statusCode,
        bool isSuccess,
        long durationMs,
        params string[] tags)
    {
        var request = CreateRequestBase(collection, url, method, tags);

        request.Responses.Add(new ApiResponse
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            Request = request,
            StatusCode = statusCode,
            IsSuccess = isSuccess,
            ReceivedAt = DateTime.UtcNow.AddMinutes(-Random.Shared.Next(2, 30)),
            DurationMs = durationMs,
            PayloadSizeBytes = 512,
            ResponseBody = isSuccess ? "{\"status\":\"ok\"}" : "{\"error\":\"conflict\"}"
        });

        request.LastExecutedAt = request.Responses[0].ReceivedAt;
        return request;
    }

    private static ApiRequest CreateRequestWithoutExecution(
        ApiCollection collection,
        string url,
        HttpMethodType method,
        params string[] tags)
    {
        return CreateRequestBase(collection, url, method, tags);
    }

    private static ApiRequest CreateRequestBase(
        ApiCollection collection,
        string url,
        HttpMethodType method,
        params string[] tags)
    {
        var request = new ApiRequest
        {
            Id = Guid.NewGuid(),
            Name = url,
            Url = url,
            Method = method,
            Body = string.Empty,
            CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 15)),
            CollectionId = collection.Id,
            Collection = collection
        };

        foreach (var tagName in tags)
        {
            var tag = new RequestTag
            {
                Id = Guid.NewGuid(),
                Name = tagName,
                ColorHex = "#607D8B",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            };

            var map = new RequestTagMap
            {
                RequestId = request.Id,
                Request = request,
                TagId = tag.Id,
                Tag = tag,
                LinkedAt = DateTime.UtcNow
            };

            request.TagLinks.Add(map);
            tag.RequestLinks.Add(map);
        }

        collection.Requests.Add(request);
        return request;
    }
}