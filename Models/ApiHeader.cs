namespace API_tester.Models;

public class ApiHeader
{
    public int Id { get; set; }
    public int RequestId { get; set; }
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