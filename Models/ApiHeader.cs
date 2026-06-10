namespace API_tester.Models;

using System.ComponentModel.DataAnnotations;

public class ApiHeader
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public virtual ApiRequest? Request { get; set; }

    [Required]
    [StringLength(150)]
    public string Key { get; set; }

    [StringLength(2000)]
    public string Value { get; set; }

    public bool IsEnabled { get; set; }

    public ApiHeader()
    {
        Key = string.Empty;
        Value = string.Empty;
    }
}
