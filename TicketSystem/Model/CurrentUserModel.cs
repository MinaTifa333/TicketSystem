namespace TicketSystem.Model
{
    public class CurrentUserModel
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public int? SectionId { get; set; } // تأكد من ضبطه عند تحميل المستخدم
        public DateTime LastNotificationCheck { get; set; } = DateTime.Now.AddMinutes(-10); // وقت آخر تحقق
    }

}
