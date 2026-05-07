using Microsoft.AspNetCore.Mvc;
using API_tester.Models;
using API_tester.Models.Enums;

namespace API_tester.Controllers;

public class RequestDetailsController : Controller
{
    [HttpGet]
    public IActionResult Details(Guid requestId)
    {
        var requests = BuildRequests();
        var request = requests.FirstOrDefault(r => r.Id == requestId);

        if (request == null)
        {
            return NotFound();
        }

        // If AJAX request, return partial for modal; otherwise full page
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_RequestDetails", request);
        }

        return View(request);
    }

    // --- Demo data builders (kept local to avoid cross-file coupling) ---
    private static List<ApiRequest> BuildRequests()
    {
        var paymentsWorkspace = CreateWorkspace("Payments Core");
        var identityWorkspace = CreateWorkspace("Identity Gateway");

        var paymentsCollection = CreateCollection(paymentsWorkspace, "Payments Authorization");
        var identityCollection = CreateCollection(identityWorkspace, "Identity Session Flow");

        return new List<ApiRequest>
        {
            CreateRequest(paymentsCollection, "/v1/payments/authorize", HttpMethodType.Post, 200, true, 143, "critical", "pci", "checkout"),
            CreateRequest(identityCollection, "/v1/sessions/{sessionId}", HttpMethodType.Get, 200, true, 89, "auth", "mobile"),
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
