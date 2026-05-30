using API_tester.Models.Enums;

namespace API_tester.Models;

public class ApiRequest
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public HttpMethodType Method { get; set; }
    public string Body { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastExecutedAt { get; set; }
    public int CollectionId { get; set; }
    public virtual ApiCollection? Collection { get; set; }
    public virtual ICollection<ApiHeader> Headers { get; set; }
    public virtual ICollection<ApiResponse> Responses { get; set; }
    public virtual ICollection<RequestTagMap> TagLinks { get; set; }
    public virtual ICollection<RequestEnvironmentLink> EnvironmentLinks { get; set; }

    public ApiRequest()
    {
        Name = string.Empty;
        Url = string.Empty;
        Body = string.Empty;
        Headers = new HashSet<ApiHeader>();
        Responses = new HashSet<ApiResponse>();
        TagLinks = new HashSet<RequestTagMap>();
        EnvironmentLinks = new HashSet<RequestEnvironmentLink>();
    }
}