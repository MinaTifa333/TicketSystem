namespace TicketSystem.Model
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string? TargetSectionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;

        public int? RelatedOrderId { get; set; } // الطلب المرتبط

    }

}
