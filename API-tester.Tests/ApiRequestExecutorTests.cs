using System.Net;
using API_tester.Models;
using API_tester.Models.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace API_tester.Tests;

public class ApiRequestExecutorTests
{
    [Fact]
    public async Task ExecuteRequestAsync_ReturnsResponseForSuccessfulCall()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"ok\":true}")
        });
        var executor = new ApiRequestExecutor(new HttpClient(handler), NullLogger<ApiRequestExecutor>.Instance);
        var request = CreateRequest();

        var response = await executor.ExecuteRequestAsync(request);

        Assert.True(response.IsSuccess);
        Assert.Equal(200, response.StatusCode);
        Assert.Contains("\"ok\":true", response.ResponseBody);
        Assert.NotNull(request.LastExecutedAt);
    }

    [Fact]
    public async Task ExecuteRequestAsync_ReturnsSafeFailureWhenTransportThrows()
    {
        var handler = new StubHandler(_ => throw new HttpRequestException("network unavailable"));
        var executor = new ApiRequestExecutor(new HttpClient(handler), NullLogger<ApiRequestExecutor>.Instance);

        var response = await executor.ExecuteRequestAsync(CreateRequest());

        Assert.False(response.IsSuccess);
        Assert.Equal(0, response.StatusCode);
        Assert.Contains("network unavailable", response.ResponseBody);
    }

    private static ApiRequest CreateRequest() => new()
    {
        Id = 42,
        Name = "Test request",
        Url = "https://example.com/api",
        Method = HttpMethodType.Get,
        CollectionId = 1
    };

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(_handler(request));
    }
}
