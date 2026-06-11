using API_tester.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace API_tester.Models;

public class ApiRequest
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [Required]
    [Url]
    [StringLength(2000)]
    public string Url { get; set; }

    [Required]
    public HttpMethodType Method { get; set; }

    public string Body { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }

    public DateTime? LastExecutedAt { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a collection.")]
    public int CollectionId { get; set; }

    public virtual ApiCollection? Collection { get; set; }
    public virtual ICollection<ApiHeader> Headers { get; set; }
    public virtual ICollection<ApiResponse> Responses { get; set; }
    public virtual ICollection<RequestTagMap> TagLinks { get; set; }
    public virtual ICollection<RequestEnvironmentLink> EnvironmentLinks { get; set; }
    public virtual ICollection<RequestAttachment> Attachments { get; set; }

    public ApiRequest()
    {
        Name = string.Empty;
        Url = string.Empty;
        Body = string.Empty;
        Headers = new HashSet<ApiHeader>();
        Responses = new HashSet<ApiResponse>();
        TagLinks = new HashSet<RequestTagMap>();
        EnvironmentLinks = new HashSet<RequestEnvironmentLink>();
        Attachments = new HashSet<RequestAttachment>();
    }
}
