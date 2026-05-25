namespace BagsOn.Models
{
    public class FilterChip
    {
        public string Label { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public FilterOption? Option { get; set; }
    }
}