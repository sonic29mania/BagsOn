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