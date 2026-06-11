using System.ComponentModel.DataAnnotations;
using API_tester.Models.Enums;

namespace API_tester.Dtos;

public record WorkspaceDto(int Id, string Name, string Description, DateTime CreatedAt, int CollectionCount, int EnvironmentCount);
public record WorkspaceWriteDto([Required, StringLength(150)] string Name, [StringLength(2000)] string? Description);

public record CollectionSummaryDto(int Id, string Name);
public record WorkspaceSummaryDto(int Id, string Name);
public record EnvironmentSummaryDto(int Id, string Name, string BaseUrl);
public record TagSummaryDto(int Id, string Name, string ColorHex);
public record HeaderDto(int Id, int RequestId, string Key, string Value, bool IsEnabled);
public record AttachmentDto(int Id, int RequestId, string FileName, string FilePath, string ContentType, long FileSize, DateTime CreatedAt);
public record AttachmentWriteDto([Required, StringLength(260)] string FileName);

public record CollectionDto(int Id, string Name, string Description, bool IsShared, DateTime CreatedAt, WorkspaceSummaryDto? Workspace, int RequestCount);
public record CollectionWriteDto(
    [Required, StringLength(200)] string Name,
    [StringLength(1000)] string? Description,
    bool IsShared,
    [Range(1, int.MaxValue)] int WorkspaceId);

public record EnvironmentDto(int Id, string Name, string BaseUrl, EnvironmentType Type, bool IsActive, DateTime CreatedAt, WorkspaceSummaryDto? Workspace);
public record EnvironmentWriteDto(
    [Required, StringLength(150)] string Name,
    [Required, Url, StringLength(2000)] string BaseUrl,
    EnvironmentType Type,
    bool IsActive,
    [Range(1, int.MaxValue)] int WorkspaceId);

public record EnvironmentVariableDto(int Id, int EnvironmentId, string Key, string Value, bool IsSecret, DateTime LastUpdatedAt);
public record EnvironmentVariableWriteDto(
    [Range(1, int.MaxValue)] int EnvironmentId,
    [Required, StringLength(150)] string Key,
    [Required, StringLength(4000)] string Value,
    bool IsSecret,
    DateTime? LastUpdatedAt);

public record RequestDto(
    int Id,
    string Name,
    string Url,
    HttpMethodType Method,
    string Body,
    DateTime CreatedAt,
    DateTime? LastExecutedAt,
    CollectionSummaryDto? Collection,
    IReadOnlyList<HeaderDto> Headers,
    IReadOnlyList<TagSummaryDto> Tags,
    EnvironmentSummaryDto? Environment,
    IReadOnlyList<AttachmentDto> Attachments);

public record RequestWriteDto(
    [Required, StringLength(200)] string Name,
    [Required, Url, StringLength(2000)] string Url,
    HttpMethodType Method,
    string? Body,
    [Range(1, int.MaxValue)] int CollectionId,
    int? EnvironmentId,
    IReadOnlyList<int>? TagIds);

public record HeaderWriteDto(
    [Range(1, int.MaxValue)] int RequestId,
    [Required, StringLength(150)] string Key,
    [StringLength(2000)] string? Value,
    bool IsEnabled);

public record TagDto(int Id, string Name, string ColorHex, DateTime CreatedAt);
public record TagWriteDto(
    [Required, StringLength(80)] string Name,
    [Required, RegularExpression("^#([A-Fa-f0-9]{6})$")] string ColorHex);
