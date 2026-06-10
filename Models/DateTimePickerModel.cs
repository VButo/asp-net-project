using System;

namespace API_tester.Models
{
    public class DateTimePickerModel
    {
        public string InputId { get; set; } = string.Empty;
        public string InputName { get; set; } = string.Empty;
        public DateTime? Value { get; set; }
        public string Placeholder { get; set; } = string.Empty;
        public bool ShowTime { get; set; } = true;
        public string CssClass { get; set; } = "form-control";
        public bool Required { get; set; } = false;
    }
}
