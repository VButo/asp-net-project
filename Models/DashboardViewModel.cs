using API_tester.Models.Enums;

namespace API_tester.Models;

public class DashboardViewModel
{
    public int TotalWorkspaces { get; set; }
    public int RecentWorkspaces { get; set; }
    public int TotalCollections { get; set; }
    public int TotalRequests { get; set; }
    public DateTime? LastRunAt { get; set; }
    public long? LastDurationMs { get; set; }
    public int? LastStatusCode { get; set; }
    public bool LastRunSucceeded { get; set; }
    public DashboardRequestPreview? RequestPreview { get; set; }
    public DashboardEnvironmentSummary? ActiveEnvironment { get; set; }
    public IReadOnlyList<DashboardWorkspaceSummary> Workspaces { get; set; } = Array.Empty<DashboardWorkspaceSummary>();
    public IReadOnlyList<DashboardCollectionSummary> Collections { get; set; } = Array.Empty<DashboardCollectionSummary>();
}

public class DashboardWorkspaceSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CollectionCount { get; set; }
    public int RequestCount { get; set; }
}

public class DashboardCollectionSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<HttpMethodType> Methods { get; set; } = Array.Empty<HttpMethodType>();
    public int RequestCount { get; set; }
}

public class DashboardEnvironmentSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public int VariableCount { get; set; }
    public int SecretCount { get; set; }
}

public class DashboardRequestPreview
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public HttpMethodType Method { get; set; }
    public string Body { get; set; } = string.Empty;
}
