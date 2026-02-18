using System.ComponentModel.DataAnnotations;

namespace TicketSystem.TSModel
{
    public class Rate
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "اختيار Order Fixed مطلوب")]
        public int? OrderFixedId { get; set; }

        [Required(ErrorMessage = "اختيار القسم مطلوب")]
        public string FromDepartment { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "الوقت يجب أن يكون أكبر من 0")]
        public int TimeMinute { get; set; }

        [Range(0.1, double.MaxValue, ErrorMessage = "القيمة يجب أن تكون أكبر من 0")]
        public decimal RateValue { get; set; }

        public OrderFixed? OrderFixed { get; set; }
    }

}
