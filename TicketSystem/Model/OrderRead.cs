using TicketSystem.Data;
using TicketSystem.TSModel;

namespace TicketSystem.Model
{
    public class OrderRead
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string UserId { get; set; }

        public Order Order { get; set; }
        public ApplicationUser User { get; set; }
    }
}
