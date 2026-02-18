using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TicketSystem.TSModel;

public partial class OrderDetail
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    [Required(ErrorMessage = "يرجى اختيار القسم المرسل منه")]
    public string FromDep { get; set; }

    [Required(ErrorMessage = "يرجى اختيار القسم المرسل إليه")]
    public string ToDepartment { get; set; }

    public bool NeedApproval { get; set; }

    public string? Attachment { get; set; }

    public string? Notes { get; set; }

    public string? QueueNumber { get; set; }

    public string? PcName { get; set; }

    public double? Amount { get; set; }


    public string? RecieptStatus { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreationDate { get; set; }
    public bool IsRelatedToCurrentSection { get; set; } = false;

    public virtual Order Order { get; set; } = null!;
    public double? Rate { get; set; }

}
