using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using API_tester.Models;
using API_tester.Models.Enums;

namespace API_tester.Services;

public class RequestDraftService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RequestDraftService> _logger;

    public RequestDraftService(HttpClient httpClient, IConfiguration configuration, ILogger<RequestDraftService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AiRequestDraft> CreateDraftAsync(string prompt, CancellationToken cancellationToken)
    {
        var apiKey = _configuration["AI:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            try
            {
                return await CreateProviderDraftAsync(prompt, apiKey, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "AI provider request failed; using the local request-draft fallback.");
            }
        }

        return CreateLocalDraft(prompt, string.IsNullOrWhiteSpace(apiKey)
            ? "Local fallback used. Configure AI__ApiKey to enable provider-generated drafts."
            : "The AI provider was unavailable, so a local fallback draft was generated.");
    }

    private async Task<AiRequestDraft> CreateProviderDraftAsync(string prompt, string apiKey, CancellationToken cancellationToken)
    {
        var endpoint = _configuration["AI:Endpoint"] ?? "https://api.openai.com/v1/chat/completions";
        var model = _configuration["AI:Model"] ?? "gpt-4.1-mini";
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new
        {
            model,
            response_format = new { type = "json_object" },
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "Return JSON only with name, method, url, headers (object), and body. Create a safe HTTP request draft from the user's description. Never include credentials."
                },
                new { role = "user", content = prompt }
            }
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var content = payload.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("AI provider returned an empty draft.");
        }

        using var draftJson = JsonDocument.Parse(content);
        var root = draftJson.RootElement;
        var method = ParseMethod(root.TryGetProperty("method", out var methodElement) ? methodElement.GetString() : null);
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (root.TryGetProperty("headers", out var headerElement) && headerElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in headerElement.EnumerateObject())
            {
                headers[property.Name] = property.Value.GetString() ?? string.Empty;
            }
        }

        return new AiRequestDraft(
            root.TryGetProperty("name", out var name) ? name.GetString() ?? "AI request" : "AI request",
            ToMethodValue(method),
            root.TryGetProperty("url", out var url) ? url.GetString() ?? string.Empty : string.Empty,
            headers,
            root.TryGetProperty("body", out var body) ? body.GetString() ?? string.Empty : string.Empty,
            "provider");
    }

    private static AiRequestDraft CreateLocalDraft(string prompt, string message)
    {
        var normalized = prompt.Trim();
        var supportedMethods = Enum.GetValues<HttpMethodType>();
        var explicitMethod = supportedMethods
            .FirstOrDefault(candidate => normalized.Contains(candidate.ToString(), StringComparison.OrdinalIgnoreCase));
        var hasExplicitMethod = supportedMethods.Any(candidate =>
            normalized.Contains(candidate.ToString(), StringComparison.OrdinalIgnoreCase));
        var method = hasExplicitMethod ? explicitMethod : HttpMethodType.Get;

        var urlMatch = System.Text.RegularExpressions.Regex.Match(normalized, @"https?://[^\s""']+");
        var url = urlMatch.Success
            ? urlMatch.Value.TrimEnd('.', ',', ';')
            : normalized.Contains("jsonplaceholder", StringComparison.OrdinalIgnoreCase)
                ? "https://jsonplaceholder.typicode.com/users"
                : "https://api.example.com/resource";

        if (!hasExplicitMethod
            && normalized.Contains("create", StringComparison.OrdinalIgnoreCase)
            && method == HttpMethodType.Get)
        {
            method = HttpMethodType.Post;
        }

        var body = method is HttpMethodType.Post or HttpMethodType.Put or HttpMethodType.Patch ? "{\n  \n}" : string.Empty;
        var headers = body.Length > 0
            ? new Dictionary<string, string> { ["Content-Type"] = "application/json" }
            : new Dictionary<string, string>();

        var name = normalized.Length <= 80 ? normalized : normalized[..77] + "...";
        return new AiRequestDraft(name, ToMethodValue(method), url, headers, body, "local", message);
    }

    private static HttpMethodType ParseMethod(string? value) =>
        Enum.TryParse<HttpMethodType>(value, true, out var method) ? method : HttpMethodType.Get;

    private static string ToMethodValue(HttpMethodType method) =>
        method.ToString().ToUpperInvariant();
}
