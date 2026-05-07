using Microsoft.AspNetCore.Mvc;
using API_tester.Models;
using API_tester.Models.Enums;

namespace API_tester.Controllers;

public class CollectionController : Controller
{
    public IActionResult Index()
    {
        var collections = BuildCollectionList();

        ViewData["Title"] = "Collections";
        ViewData["BreadcrumbCurrent"] = "Collections";
        ViewData["HeroKicker"] = "Collection Catalog";
        ViewData["HeroTitle"] = "Organiziraj endpoint flowove kroz pregledne API collectione.";
        ViewData["HeroDescription"] = "Centralni pregled collectiona za sve timove. Svaka kartica pokazuje naziv, opis, workspace kojem pripada i broj requestova.";
        ViewData["NewCollectionActionText"] = "+ New Collection";
        ViewData["ImportCollectionActionText"] = "Import Collection";
        ViewData["SearchPlaceholder"] = "Search by collection name, workspace or domain";
        ViewData["Filters"] = new List<string> { "All", "Shared", "Private" };

        return View(collections);
    }

    public IActionResult Details()
    {
        var collection = BuildCollectionDetails();

        ViewData["Title"] = "Collection Details";
        ViewData["BreadcrumbCurrent"] = "Collection Details";
        ViewData["HeroKicker"] = "Collection Details";
        ViewData["HeroTitle"] = collection.Name;
        ViewData["HeroDescription"] = collection.Description;
        ViewData["PrimaryActionText"] = "Open Requests";
        ViewData["SecondaryActionText"] = "Back to Workspace";

        return View(collection);
    }

    private static ApiCollection BuildCollectionDetails()
    {
        var workspace = new ApiWorkspace
        {
            Id = Guid.NewGuid(),
            Name = "Payments Core",
            Description = "Centralni workspace za payment autorizaciju, refund, settlement i antifraud request tokove.",
            CreatedAt = DateTime.UtcNow.AddMonths(-2),
            OwnerUserId = Guid.NewGuid(),
            OwnerUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "Vedran T.",
                Email = "vedran@apitester.dev",
                Role = "Owner",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            }
        };

        var collection = new ApiCollection
        {
            Id = Guid.NewGuid(),
            Name = "Payments Authorization",
            Description = "Collection za authorize, capture i refund tokove sa svim edge-case response scenarijima.",
            CreatedAt = DateTime.UtcNow.AddDays(-18),
            IsShared = true,
            WorkspaceId = workspace.Id,
            Workspace = workspace
        };

        workspace.Collections.Add(collection);

        var staging = new ApiEnvironment
        {
            Id = Guid.NewGuid(),
            Name = "staging-eu",
            Type = EnvironmentType.Staging,
            BaseUrl = "https://staging.api.company.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-12),
            WorkspaceId = workspace.Id,
            Workspace = workspace
        };

        var prod = new ApiEnvironment
        {
            Id = Guid.NewGuid(),
            Name = "prod-readonly",
            Type = EnvironmentType.Production,
            BaseUrl = "https://api.company.com",
            IsActive = false,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            WorkspaceId = workspace.Id,
            Workspace = workspace
        };

        workspace.Environments.Add(staging);
        workspace.Environments.Add(prod);

        var criticalTag = new RequestTag
        {
            Id = Guid.NewGuid(),
            Name = "critical",
            ColorHex = "#D32F2F",
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        };

        var pciTag = new RequestTag
        {
            Id = Guid.NewGuid(),
            Name = "pci",
            ColorHex = "#1976D2",
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        };

        var captureTag = new RequestTag
        {
            Id = Guid.NewGuid(),
            Name = "capture",
            ColorHex = "#2E7D32",
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        };

        var checkoutTag = new RequestTag
        {
            Id = Guid.NewGuid(),
            Name = "checkout",
            ColorHex = "#6A1B9A",
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        };

        var refundTag = new RequestTag
        {
            Id = Guid.NewGuid(),
            Name = "refund",
            ColorHex = "#F57C00",
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        };

        var manualReviewTag = new RequestTag
        {
            Id = Guid.NewGuid(),
            Name = "manual-review",
            ColorHex = "#455A64",
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        };

        CreateRequest(collection, staging, "/v1/payments/authorize", HttpMethodType.Post, "Authorize payment", "{\"amount\":129.90,\"currency\":\"EUR\"}", 200, true, 143, 512, criticalTag, pciTag);
        CreateRequest(collection, staging, "/v1/payments/capture", HttpMethodType.Post, "Capture payment", "{\"paymentId\":\"pay_92311\"}", 201, true, 166, 488, captureTag, checkoutTag);
        CreateRequest(collection, prod, "/v1/payments/refund", HttpMethodType.Post, "Refund payment", "{\"paymentId\":\"pay_92311\"}", 409, false, 302, 377, refundTag, manualReviewTag);

        return collection;
    }

    private static List<ApiCollection> BuildCollectionList()
    {
        var paymentsWorkspace = new ApiWorkspace
        {
            Id = Guid.NewGuid(),
            Name = "Payments Core",
            Description = "Payment processing workspace",
            CreatedAt = DateTime.UtcNow.AddDays(-45)
        };

        var identityWorkspace = new ApiWorkspace
        {
            Id = Guid.NewGuid(),
            Name = "Identity Gateway",
            Description = "Identity and session workspace",
            CreatedAt = DateTime.UtcNow.AddDays(-40)
        };

        var ordersWorkspace = new ApiWorkspace
        {
            Id = Guid.NewGuid(),
            Name = "Orders Public API",
            Description = "Partner facing orders workspace",
            CreatedAt = DateTime.UtcNow.AddDays(-35)
        };

        var notificationsWorkspace = new ApiWorkspace
        {
            Id = Guid.NewGuid(),
            Name = "Notifications Hub",
            Description = "Notifications workspace",
            CreatedAt = DateTime.UtcNow.AddDays(-32)
        };

        var fraudWorkspace = new ApiWorkspace
        {
            Id = Guid.NewGuid(),
            Name = "Fraud Engine",
            Description = "Fraud and scoring workspace",
            CreatedAt = DateTime.UtcNow.AddDays(-28)
        };

        var sandboxWorkspace = new ApiWorkspace
        {
            Id = Guid.NewGuid(),
            Name = "Developer Sandbox",
            Description = "Internal sandbox workspace",
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };

        return new List<ApiCollection>
        {
            CreateCollectionCard(paymentsWorkspace, "Payments Authorization", "Authorize, capture i refund scenariji za karticna placanja, ukljucujuci fallback i timeout varijante.", true, 18),
            CreateCollectionCard(identityWorkspace, "Identity Session Flow", "Login, refresh token i session revoke requestovi za web, mobile i internal admin klijente.", true, 14),
            CreateCollectionCard(ordersWorkspace, "Orders Partner API", "Create, update i status endpointovi za partnere, s conflict handling i retry test putanjama.", false, 21),
            CreateCollectionCard(notificationsWorkspace, "Notification Dispatch", "Email, SMS i push payload varijante sa status callback validacijom i retry granama.", true, 16),
            CreateCollectionCard(fraudWorkspace, "Fraud Rules and Scoring", "Risk scoring, velocity check i denylist endpointovi za detekciju sumnjivih transakcija.", false, 27),
            CreateCollectionCard(sandboxWorkspace, "Sandbox Smoke Set", "Minimalni smoke requestovi za health endpointove i brzu validaciju payload shema prije mergea.", false, 9)
        };
    }

    private static ApiCollection CreateCollectionCard(ApiWorkspace workspace, string name, string description, bool isShared, int requestCount)
    {
        var collection = new ApiCollection
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow.AddDays(-requestCount),
            IsShared = isShared,
            WorkspaceId = workspace.Id,
            Workspace = workspace
        };

        for (var index = 0; index < requestCount; index++)
        {
            collection.Requests.Add(new ApiRequest
            {
                Id = Guid.NewGuid(),
                Name = $"Request {index + 1}",
                Url = "/placeholder",
                CreatedAt = DateTime.UtcNow.AddMinutes(-index),
                CollectionId = collection.Id,
                Collection = collection
            });
        }

        workspace.Collections.Add(collection);
        return collection;
    }

    private static ApiRequest CreateRequest(
        ApiCollection collection,
        ApiEnvironment environment,
        string url,
        HttpMethodType method,
        string name,
        string body,
        int statusCode,
        bool isSuccess,
        long durationMs,
        int payloadSizeBytes,
        params RequestTag[] tags)
    {
        var request = new ApiRequest
        {
            Id = Guid.NewGuid(),
            Name = name,
            Url = url,
            Method = method,
            Body = body,
            CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 10)),
            CollectionId = collection.Id,
            Collection = collection,
            LastExecutedAt = DateTime.UtcNow.AddMinutes(-Random.Shared.Next(1, 25))
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

        var link = new RequestEnvironmentLink
        {
            RequestId = request.Id,
            Request = request,
            EnvironmentId = environment.Id,
            Environment = environment,
            LinkedAt = DateTime.UtcNow,
            IsDefaultEnvironment = environment.Type == EnvironmentType.Development
        };

        request.EnvironmentLinks.Add(link);
        environment.RequestLinks.Add(link);

        foreach (var tag in tags)
        {
            var tagMap = new RequestTagMap
            {
                RequestId = request.Id,
                Request = request,
                TagId = tag.Id,
                Tag = tag,
                LinkedAt = DateTime.UtcNow
            };

            request.TagLinks.Add(tagMap);
            tag.RequestLinks.Add(tagMap);
        }

        request.Responses.Add(new ApiResponse
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            Request = request,
            StatusCode = statusCode,
            IsSuccess = isSuccess,
            ReceivedAt = DateTime.UtcNow.AddMinutes(-10),
            DurationMs = durationMs,
            PayloadSizeBytes = payloadSizeBytes,
            ResponseBody = isSuccess
                ? "{\n  \"status\": \"authorized\"\n}"
                : "{\n  \"error\": \"conflict\"\n}"
        });

        return request;
    }
}