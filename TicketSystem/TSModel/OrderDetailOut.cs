namespace TicketSystem.TSModel
{
    public class OrderDetailOut
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public string? Nid { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }



        public virtual Order Order { get; set; } = null!;

    }
}
