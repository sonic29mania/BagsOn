using System;
/// Клас Order описує замовлення покупця. 
namespace BagsOn.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public DateTime OrderDate { get; set; }

        public TimeSpan OrderTime { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public TimeSpan? DeliveryTime { get; set; }

        public string DeliveryAddress { get; set; } = string.Empty;

        public string Comment { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public bool IsActive { get; set; }

        public int? CustomerId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public int StatusId { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public int DeliveryTypeId { get; set; }

        public string DeliveryTypeName { get; set; } = string.Empty;

        public int ItemsCount { get; set; }

        public string ArchiveType
        {
            get
            {
                if (StatusName == "Виконано")
                {
                    return "Виконане";
                }

                if (StatusName == "Скасовано")
                {
                    return "Скасоване";
                }

                if (IsArchived)
                {
                    return "Ручно архівоване";
                }

                return "Архівне";
            }
        }
        public DateTime OrderDateTime
        {
            get
            {
                return OrderDate.Date.Add(OrderTime);
            }
        }

        public int UrgencyLevel
        {
            get
            {
                TimeSpan waitingTime = DateTime.Now - OrderDateTime;

                if (StatusName == "Оплачено" && waitingTime.TotalDays > 2)
                {
                    return 2;
                }

                if (StatusName == "Нове" && waitingTime.TotalHours > 24)
                {
                    return 1;
                }

                if (StatusName == "Очікує оплати" && waitingTime.TotalHours > 24)
                {
                    return 1;
                }

                if (StatusName == "Відправлено" && waitingTime.TotalDays > 5)
                {
                    return 1;
                }

                return 0;
            }
        }

        public string UrgencyText
        {
            get
            {
                TimeSpan waitingTime = DateTime.Now - OrderDateTime;

                if (StatusName == "Оплачено" && waitingTime.TotalDays > 2)
                {
                    return "Оплачено понад 2 дні";
                }

                if (StatusName == "Нове" && waitingTime.TotalHours > 24)
                {
                    return "Нове понад 24 год";
                }

                if (StatusName == "Очікує оплати" && waitingTime.TotalHours > 24)
                {
                    return "Очікує оплати понад 24 год";
                }

                if (StatusName == "Відправлено" && waitingTime.TotalDays > 5)
                {
                    return "Відправлено понад 5 днів";
                }

                return "";
            }
        }
        public string CustomerComment { get; set; } = string.Empty;

        public string ManagerComment { get; set; } = string.Empty;

        public string CustomerAddress { get; set; } = string.Empty;
        public bool IsArchived { get; set; }

        public DateTime? ArchivedAt { get; set; }

        public string ArchiveReason { get; set; } = string.Empty;
    }
}