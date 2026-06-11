using System.ComponentModel.DataAnnotations;

namespace API_tester.Models;

public class RequestAttachment
{
    public int Id { get; set; }

    public int RequestId { get; set; }
    public virtual ApiRequest? Request { get; set; }

    [Required]
    [StringLength(260)]
    public string FileName { get; set; }

    [Required]
    [StringLength(260)]
    public string StoredFileName { get; set; }

    [Required]
    [StringLength(1000)]
    public string FilePath { get; set; }

    [StringLength(200)]
    public string ContentType { get; set; }

    public long FileSize { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }

    public RequestAttachment()
    {
        FileName = string.Empty;
        StoredFileName = string.Empty;
        FilePath = string.Empty;
        ContentType = string.Empty;
        CreatedAt = DateTime.UtcNow;
    }
}
