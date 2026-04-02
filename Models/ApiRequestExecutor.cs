using API_tester.Models.Enums;

namespace API_tester.Models;

public class ApiRequestExecutor
{
    private readonly Random _random = new();

    public async Task<ApiResponse> ExecuteRequestAsync(ApiRequest request)
    {
        var delay = _random.Next(120, 550);
        await Task.Delay(delay);

        var statusCode = request.Method == HttpMethodType.Get
            ? 200
            : _random.Next(0, 10) < 8 ? 200 : 500;

        var response = new ApiResponse
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            Request = request,
            StatusCode = statusCode,
            IsSuccess = statusCode is >= 200 and < 300,
            ReceivedAt = DateTime.UtcNow,
            DurationMs = delay,
            PayloadSizeBytes = _random.Next(120, 1500),
            ResponseBody = statusCode >= 400
                ? "{\"error\":\"Simulated failure\"}"
                : "{\"result\":\"Simulated success\"}"
        };

        request.LastExecutedAt = response.ReceivedAt;
        request.Responses.Add(response);

        return response;
    }
}