namespace API_tester.Models;

using System.ComponentModel.DataAnnotations;

public class RequestTag
{
    public int Id { get; set; }

    [Required]
    [StringLength(80)]
    public string Name { get; set; }

    [Required]
    [RegularExpression("^#([A-Fa-f0-9]{6})$", ErrorMessage = "Use a color like #13D0D4.")]
    public string ColorHex { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }
    public virtual ICollection<RequestTagMap> RequestLinks { get; set; }

    public RequestTag()
    {
        Name = string.Empty;
        ColorHex = string.Empty;
        RequestLinks = new HashSet<RequestTagMap>();
    }
}
