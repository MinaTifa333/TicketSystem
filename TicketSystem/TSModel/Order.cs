using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSystem.TSModel;

public partial class Order
{
    public int Id { get; set; } // معرف الطلب

    public string? Nid { get; set; } // رقم الهوية الوطنية ملهوش لازمة

    [Required(ErrorMessage = "يجب اختيار الخدمة.")]
    public int? FixedId { get; set; }// معرف الطلب الثابت

    public bool? Closed { get; set; } // حالة الطلب (مغلق أم لا)

    public string? Notes { get; set; } // ملاحظات حول الطلب

    public string? QueueNumber { get; set; } = null;   // رقم الدور في الطابور

    public string? CreatedBy { get; set; }// اسم المستخدم الذي أنشأ الطلب

    public DateTime? DateTime { get; set; } // تاريخ ووقت إنشاء الطلب

    public string? PcName { get; set; } // اسم الكمبيوتر الذي تم إنشاء الطلب منه

    public string? LastDepartment { get; set; } // آخر قسم تم التعامل مع الطلب فيه

    public int? RepeateFactor { get; set; } // عامل التكرار للطلب (إذا كان الطلب مكررًا)

    public byte[]? SsmaTimeStamp { get; set; }// الطابع الزمني للطلب (للتزامن) ملهوش لازمة

    [Required]
    public int SectionId { get; set; }

    // العلاقات
    [ForeignKey(nameof(SectionId))]
    public virtual Section? Section { get; set; } // العلاقة الاختيارية إذا كنت تريد تحميل معلومات القسم
    public virtual ICollection<RawsData> RawsDataRecords { get; set; } = new List<RawsData>();

    public virtual OrderFixed? Fixed { get; set; }


    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public bool IsRead { get; set; } = false; 
    [NotMapped]
    public bool IsReadByCurrentUser { get; set; } = false;

    public OrderDetailOut? ExternalDetail { get; set; }

}
