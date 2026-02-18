using TicketSystem.Data;

namespace TicketSystem.TSModel
{
    public class UserSection
    {
        public string UserId { get; set; } = string.Empty;
        public int SectionId { get; set; }

        public ApplicationUser User { get; set; }
        public Section Section { get; set; }
    }
}
