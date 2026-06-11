using API_tester.Dtos;
using API_tester.Models;

namespace API_tester.Controllers.Api;

internal static class ApiMapping
{
    public static WorkspaceDto ToDto(this ApiWorkspace workspace) =>
        new(workspace.Id, workspace.Name, workspace.Description, workspace.CreatedAt, workspace.Collections.Count, workspace.Environments.Count);

    public static CollectionDto ToDto(this ApiCollection collection) =>
        new(
            collection.Id,
            collection.Name,
            collection.Description,
            collection.IsShared,
            collection.CreatedAt,
            collection.Workspace == null ? null : new WorkspaceSummaryDto(collection.Workspace.Id, collection.Workspace.Name),
            collection.Requests.Count);

    public static EnvironmentDto ToDto(this ApiEnvironment environment) =>
        new(
            environment.Id,
            environment.Name,
            environment.BaseUrl,
            environment.Type,
            environment.IsActive,
            environment.CreatedAt,
            environment.Workspace == null ? null : new WorkspaceSummaryDto(environment.Workspace.Id, environment.Workspace.Name));

    public static EnvironmentVariableDto ToDto(this EnvironmentVariable variable) =>
        new(variable.Id, variable.EnvironmentId, variable.Key, variable.Value, variable.IsSecret, variable.LastUpdatedAt);

    public static HeaderDto ToDto(this ApiHeader header) =>
        new(header.Id, header.RequestId, header.Key, header.Value, header.IsEnabled);

    public static TagDto ToDto(this RequestTag tag) =>
        new(tag.Id, tag.Name, tag.ColorHex, tag.CreatedAt);

    public static TagSummaryDto ToSummaryDto(this RequestTag tag) =>
        new(tag.Id, tag.Name, tag.ColorHex);

    public static AttachmentDto ToDto(this RequestAttachment attachment) =>
        new(attachment.Id, attachment.RequestId, attachment.FileName, attachment.FilePath, attachment.ContentType, attachment.FileSize, attachment.CreatedAt);

    public static RequestDto ToDto(this ApiRequest request)
    {
        var defaultEnvironment = request.EnvironmentLinks
            .OrderByDescending(link => link.IsDefaultEnvironment)
            .Select(link => link.Environment)
            .FirstOrDefault(environment => environment != null);

        return new RequestDto(
            request.Id,
            request.Name,
            request.Url,
            request.Method,
            request.Body,
            request.CreatedAt,
            request.LastExecutedAt,
            request.Collection == null ? null : new CollectionSummaryDto(request.Collection.Id, request.Collection.Name),
            request.Headers.OrderBy(header => header.Id).Select(header => header.ToDto()).ToList(),
            request.TagLinks.Select(link => link.Tag).Where(tag => tag != null).Select(tag => tag!.ToSummaryDto()).ToList(),
            defaultEnvironment == null ? null : new EnvironmentSummaryDto(defaultEnvironment.Id, defaultEnvironment.Name, defaultEnvironment.BaseUrl),
            request.Attachments.OrderByDescending(a => a.CreatedAt).Select(a => a.ToDto()).ToList());
    }
}
