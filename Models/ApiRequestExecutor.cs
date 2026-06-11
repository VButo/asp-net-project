using API_tester.Models.Enums;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace API_tester.Models;

public class ApiRequestExecutor
{
    private readonly HttpClient _httpClient;

    public ApiRequestExecutor(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse> ExecuteRequestAsync(ApiRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var message = new HttpRequestMessage(ToHttpMethod(request.Method), request.Url);
            var requestHasBody = CanSendBody(request.Method) && !string.IsNullOrEmpty(request.Body);

            foreach (var header in request.Headers.Where(header => header.IsEnabled && !string.IsNullOrWhiteSpace(header.Key)))
            {
                if (!message.Headers.TryAddWithoutValidation(header.Key, header.Value))
                {
                    if (requestHasBody)
                    {
                        message.Content ??= new StringContent(request.Body, Encoding.UTF8);
                        message.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
            }

            if (requestHasBody)
            {
                var contentType = request.Headers
                    .FirstOrDefault(header => header.IsEnabled && header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    ?.Value;

                message.Content ??= new StringContent(request.Body, Encoding.UTF8);
                if (!string.IsNullOrWhiteSpace(contentType) && MediaTypeHeaderValue.TryParse(contentType, out var parsedContentType))
                {
                    message.Content.Headers.ContentType = parsedContentType;
                }
                else
                {
                    message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                }
            }

            using var httpResponse = await _httpClient.SendAsync(message);
            var responseBody = await httpResponse.Content.ReadAsStringAsync();
            stopwatch.Stop();

            return BuildResponse(request, (int)httpResponse.StatusCode, httpResponse.IsSuccessStatusCode, stopwatch.ElapsedMilliseconds, responseBody);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return BuildResponse(request, 0, false, stopwatch.ElapsedMilliseconds, $"{{\"error\":\"{EscapeJson(ex.Message)}\"}}");
        }
    }

    private static ApiResponse BuildResponse(ApiRequest request, int statusCode, bool isSuccess, long durationMs, string responseBody)
    {
        var body = responseBody ?? string.Empty;

        var response = new ApiResponse
        {
            Id = 0,
            RequestId = request.Id,
            Request = request,
            StatusCode = statusCode,
            IsSuccess = isSuccess,
            ReceivedAt = DateTime.UtcNow,
            DurationMs = durationMs,
            PayloadSizeBytes = Encoding.UTF8.GetByteCount(body),
            ResponseBody = body
        };

        request.LastExecutedAt = response.ReceivedAt;
        request.Responses.Add(response);

        return response;
    }

    private static HttpMethod ToHttpMethod(HttpMethodType method) =>
        method switch
        {
            HttpMethodType.Post => HttpMethod.Post,
            HttpMethodType.Put => HttpMethod.Put,
            HttpMethodType.Delete => HttpMethod.Delete,
            HttpMethodType.Patch => HttpMethod.Patch,
            _ => HttpMethod.Get
        };

    private static bool CanSendBody(HttpMethodType method) =>
        method is HttpMethodType.Post or HttpMethodType.Put or HttpMethodType.Patch or HttpMethodType.Delete;

    private static string EscapeJson(string value) =>
        value.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
