namespace API_tester.Models;

public class ApiHeader
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public ApiRequest? Request { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public bool IsEnabled { get; set; }

    public ApiHeader()
    {
        Key = string.Empty;
        Value = string.Empty;
    }
}