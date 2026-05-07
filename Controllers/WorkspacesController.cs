using Microsoft.AspNetCore.Mvc;
using API_tester.Models;

namespace API_tester.Controllers;

public class WorkspacesController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Workspaces";
        ViewData["BreadcrumbCurrent"] = "Workspaces";
        ViewData["HeroKicker"] = "Workspace Registry";
        ViewData["HeroTitle"] = "Organiziraj API domene kao timske radne prostore.";
        ViewData["HeroDescription"] = "Pregledaj sve workspaceove s vlasnistvom i strukturom. Svaka kartica odmah pokazuje tko je owner i koliko ima collectiona i environmenta.";
        ViewData["PrimaryActionText"] = "+ Novi Workspace";
        ViewData["SecondaryActionText"] = "Import Workspace";
        ViewData["SearchPlaceholder"] = "Search by name, owner or tag";
        ViewData["Filters"] = new List<string> { "Svi", "My Team", "Production", "Sandbox" };

        return View(BuildWorkspaces());
    }

    private static List<ApiWorkspace> BuildWorkspaces()
    {
        var payments = CreateWorkspace(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Payments Core",
            "Vedran T.",
            "Glavni workspace za payment flow, autorizaciju, refund i settlement endpointove.",
            6,
            3);

        var identity = CreateWorkspace(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Identity Gateway",
            "Ana R.",
            "Auth, token lifecycle i session endpointovi za web i mobile klijente.",
            4,
            2);

        var orders = CreateWorkspace(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "Orders Public API",
            "Marko S.",
            "Order create/update/status endpointovi za partnere i javne integracije.",
            5,
            4);

        var notifications = CreateWorkspace(
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            "Notifications Hub",
            "Petra K.",
            "Email/SMS/push delivery endpointovi i webhook callback test scenariji.",
            3,
            2);

        var fraud = CreateWorkspace(
            Guid.Parse("55555555-5555-5555-5555-555555555555"),
            "Fraud Engine",
            "Luka M.",
            "Risk scoring, blacklist checks i event stream endpointovi za anti-fraud pipeline.",
            7,
            3);

        var sandbox = CreateWorkspace(
            Guid.Parse("66666666-6666-6666-6666-666666666666"),
            "Developer Sandbox",
            "Team API Platform",
            "Siguran prostor za prototipiranje novih ruta i test payloadova prije mergea.",
            8,
            5);

        return new List<ApiWorkspace> { payments, identity, orders, notifications, fraud, sandbox };
    }

    private static ApiWorkspace CreateWorkspace(Guid id, string name, string ownerName, string description, int collectionCount, int environmentCount)
    {
        var owner = new User
        {
            Id = Guid.NewGuid(),
            Username = ownerName,
            Email = $"{ownerName.Replace(" ", ".").ToLowerInvariant()}@example.com",
            CreatedAt = DateTime.UtcNow.AddMonths(-6),
            IsActive = true,
            Role = "Member"
        };

        var workspace = new ApiWorkspace
        {
            Id = id,
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow.AddDays(-45),
            OwnerUserId = owner.Id,
            OwnerUser = owner
        };

        for (var index = 0; index < collectionCount; index++)
        {
            workspace.Collections.Add(new ApiCollection
            {
                Id = Guid.NewGuid(),
                Name = $"{name} Collection {index + 1}",
                Description = $"Collection {index + 1} for {name}",
                CreatedAt = DateTime.UtcNow.AddDays(-(index + 1)),
                WorkspaceId = workspace.Id,
                Workspace = workspace
            });
        }

        for (var index = 0; index < environmentCount; index++)
        {
            workspace.Environments.Add(new ApiEnvironment
            {
                Id = Guid.NewGuid(),
                Name = index == 0 ? "staging-eu" : $"env-{index + 1}",
                BaseUrl = index == 0 ? "https://staging.api.company.com" : $"https://env{index + 1}.api.company.com",
                CreatedAt = DateTime.UtcNow.AddDays(-(index + 2)),
                WorkspaceId = workspace.Id,
                Workspace = workspace
            });
        }

        return workspace;
    }
}
