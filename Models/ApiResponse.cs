namespace API_tester.Models;

public class ApiResponse
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public ApiRequest? Request { get; set; }
    public int StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime ReceivedAt { get; set; }
    public long DurationMs { get; set; }
    public int PayloadSizeBytes { get; set; }
    public string ResponseBody { get; set; }

    public ApiResponse()
    {
        ResponseBody = string.Empty;
    }
}