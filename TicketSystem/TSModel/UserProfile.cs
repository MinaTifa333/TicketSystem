using TicketSystem.Data;

namespace TicketSystem.TSModel
{
    public class UserProfile
    {
        public int Id { get; set; }

        public string FullName { get; set; } = null!;

        public string UserId { get; set; } = null!; // FK to Identity
        public ApplicationUser? User { get; set; }

        public int? SectionId { get; set; }
        public Section? Section { get; set; }
    }
}
