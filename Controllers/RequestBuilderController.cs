using Microsoft.AspNetCore.Mvc;
using API_tester.Models;
using API_tester.Models.Enums;

namespace API_tester.Controllers;

public class RequestBuilderController : Controller
{
    [HttpGet]
    public IActionResult Index(Guid? requestId)
    {
        var requests = BuildRequests();
        ApiRequest? model = null;

        if (requestId.HasValue)
            model = requests.FirstOrDefault(r => r.Id == requestId.Value);

        if (model == null)
        {
            model = new ApiRequest
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                Url = string.Empty,
                Method = HttpMethodType.Get,
                Body = string.Empty,
                CreatedAt = DateTime.UtcNow
            };
        }

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_RequestBuilder", model);

        return View(model);
    }

    // Small demo builders used locally by the controller
    private static List<ApiRequest> BuildRequests()
    {
        var ws = CreateWorkspace("Demo Workspace");
        var coll = CreateCollection(ws, "Demo Collection");

        return new List<ApiRequest>
        {
            CreateRequest(coll, "/demo/v1/items", HttpMethodType.Get, 200, true, 85, "demo", "smoke"),
            CreateRequest(coll, "/demo/v1/items/{id}", HttpMethodType.Patch, 409, false, 320, "demo", "update")
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
