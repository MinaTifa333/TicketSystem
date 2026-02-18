using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TicketSystem.TSModel;

public partial class OrderFixed
{
    public int Id { get; set; }

    public int Types { get; set; }
    [Required(ErrorMessage = "حقل القسم مطلوب")]

    public string Level1 { get; set; } = null!;
    [Required(ErrorMessage = "حقل الخدمة مطلوب")]

    public string Level2 { get; set; } = null!;

    public string? Level3 { get; set; }

    public string? Level4 { get; set; }

    public string? Level5 { get; set; }

    public string? Level6 { get; set; }

    public string? Label { get; set; }

    public string? WorkFlow { get; set; }

    public string? Okm { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreationDate { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Rate> Rates { get; set; } = new List<Rate>();
    public virtual ICollection<RawsData> RawsDataRecords { get; set; } = new List<RawsData>();

    public string DisplayText => $"{Level1} - {Level2}- {Level3} -{Level4}- {Level5}";

}


