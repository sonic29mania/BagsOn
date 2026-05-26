/// Клас FilterChip описує активний фільтр, який показується в інтерфейсі у вигляді невеликої мітки. Він допомагає
/// користувачу бачити, які фільтри зараз застосовані, і за потреби швидко їх прибрати.
namespace BagsOn.Models
{
    public class FilterChip
    {
        public string Label { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public FilterOption? Option { get; set; }
    }
}