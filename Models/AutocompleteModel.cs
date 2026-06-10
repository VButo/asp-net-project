namespace API_tester.Models;

public class AutocompleteModel
{
    public string InputId { get; set; } = "autocompleteInput";
    public string HiddenId { get; set; } = "selectedId";
    public string SearchUrl { get; set; } = "/search";
    public int SelectedId { get; set; }
    public string SelectedText { get; set; } = string.Empty;
    public string Placeholder { get; set; } = string.Empty;
}
