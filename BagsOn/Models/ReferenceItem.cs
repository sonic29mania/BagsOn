
/// Клас ReferenceItem є універсальною моделлю для довідникових значень. Він використовується для списків у
/// ComboBox, де потрібні тільки ідентифікатор запису та назва для відображення.
namespace BagsOn.Models
{
    public class ReferenceItem
    {
        // Id запису у таблиці
        public int Id { get; set; }

        // Назва, яка буде показуватися в ComboBox
        public string Name { get; set; } = string.Empty;
    }
}