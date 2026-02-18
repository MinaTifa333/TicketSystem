namespace TicketSystem.TSModel
{
    public class RawsData
    {
        public long ID { get; set; }

        // 🔗 Order
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        // 🔗 OrderFixed
        public int OrderFixedId { get; set; }
        public virtual OrderFixed OrderFixed { get; set; }

        // 🔗 Rate
        public int RateId { get; set; }
        public virtual Rate Rate { get; set; }

        public string FromDepartmentRate { get; set; } = string.Empty;
        public int TimeMinuteRate { get; set; }

        public DateTime DateTimeStart { get; set; }
        public DateTime DateTimeClosed { get; set; }

        public bool ClosedOrder { get; set; }
        public int RepeateFactor { get; set; }
        public int TotalRate { get; set; }
    }
}
