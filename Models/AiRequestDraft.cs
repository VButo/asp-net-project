using API_tester.Models.Enums;

namespace API_tester.Models;

public record AiRequestDraft(
    string Name,
    HttpMethodType Method,
    string Url,
    IReadOnlyDictionary<string, string> Headers,
    string Body,
    string Source,
    string? Message = null);

public record AiRequestDraftInput(string Prompt);
