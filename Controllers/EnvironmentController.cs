using Microsoft.AspNetCore.Mvc;
using API_tester.Models;
using API_tester.Models.Enums;

namespace API_tester.Controllers;

public class EnvironmentController : Controller
{
    public IActionResult Index()
    {
        var workspace = BuildEnvironmentWorkspace();

        ViewData["Title"] = "Environments";
        ViewData["BreadcrumbCurrent"] = "Environments";
        ViewData["HeroKicker"] = "Environment Registry";
        ViewData["HeroTitle"] = $"{workspace.Name} Environments";
        ViewData["HeroDescription"] = "Pregled dostupnih environment konfiguracija i brzi ulaz u Request Builder.";
        ViewData["PrimaryActionText"] = "Open Builder";
        ViewData["SecondaryActionText"] = "Back to Workspace";

        return View(workspace);
    }

    private static ApiWorkspace BuildEnvironmentWorkspace()
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

        workspace.Environments.Add(CreateEnvironment(workspace, "staging-eu", EnvironmentType.Staging, "https://staging.api.company.com", true, 14, 3));
        workspace.Environments.Add(CreateEnvironment(workspace, "staging-us", EnvironmentType.Staging, "https://us-staging.api.company.com", true, 11, 2));
        workspace.Environments.Add(CreateEnvironment(workspace, "prod-readonly", EnvironmentType.Production, "https://api.company.com", false, 6, 1));

        return workspace;
    }

    private static ApiEnvironment CreateEnvironment(
        ApiWorkspace workspace,
        string name,
        EnvironmentType type,
        string baseUrl,
        bool isActive,
        int variableCount,
        int secretCount)
    {
        var environment = new ApiEnvironment
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            BaseUrl = baseUrl,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30)),
            WorkspaceId = workspace.Id,
            Workspace = workspace
        };

        for (var index = 0; index < variableCount; index++)
        {
            environment.Variables.Add(new EnvironmentVariable
            {
                Id = Guid.NewGuid(),
                EnvironmentId = environment.Id,
                Environment = environment,
                Key = $"var_{index + 1}",
                Value = index < secretCount ? "***" : "value",
                IsSecret = index < secretCount,
                LastUpdatedAt = DateTime.UtcNow.AddDays(-index)
            });
        }

        return environment;
    }
}