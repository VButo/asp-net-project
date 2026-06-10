namespace API_tester.Models;

using System.ComponentModel.DataAnnotations;

public class EnvironmentVariable
{
    public int Id { get; set; }
    public int EnvironmentId { get; set; }
    public virtual ApiEnvironment? Environment { get; set; }

    [Required]
    [StringLength(150)]
    public string Key { get; set; }

    [Required]
    [StringLength(4000)]
    public string Value { get; set; }

    public bool IsSecret { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime LastUpdatedAt { get; set; }

    public EnvironmentVariable()
    {
        Key = string.Empty;
        Value = string.Empty;
    }
}
