using Microsoft.AspNetCore.Mvc;
using API_tester.Models;

namespace API_tester.Controllers;

public class WorkspaceDetailsController : Controller
{
    [HttpGet]
    public IActionResult Details(Guid? workspaceId)
    {
        var workspaces = BuildWorkspaces();
        ApiWorkspace? workspace = null;

        if (workspaceId.HasValue)
            workspace = workspaces.FirstOrDefault(w => w.Id == workspaceId.Value);

        workspace ??= workspaces.FirstOrDefault();

        if (workspace == null)
            return NotFound();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_WorkspaceDetails", workspace);

        return View(workspace);
    }

    // Local demo builders
    private static List<ApiWorkspace> BuildWorkspaces()
    {
        var payments = new ApiWorkspace
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Payments Core",
            Description = "Centralni workspace za payment autorizaciju, refund i settlement.",
            CreatedAt = DateTime.UtcNow.AddDays(-120)
        };

        payments.Collections.Add(new ApiCollection { Id = Guid.NewGuid(), Name = "Payments Authorization", Workspace = payments });
        payments.Collections.Add(new ApiCollection { Id = Guid.NewGuid(), Name = "Transactions Core", Workspace = payments });

        var identity = new ApiWorkspace
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Identity Gateway",
            Description = "Auth and session management endpoints.",
            CreatedAt = DateTime.UtcNow.AddDays(-90)
        };

        identity.Collections.Add(new ApiCollection { Id = Guid.NewGuid(), Name = "Identity Session Flow", Workspace = identity });

        return new List<ApiWorkspace> { payments, identity };
    }
}
