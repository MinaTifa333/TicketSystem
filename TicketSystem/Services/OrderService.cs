using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Radzen;
using TicketSystem.Data;
using TicketSystem.Hubs;
using TicketSystem.Model;
using TicketSystem.TSModel;
using static TicketSystem.Components.Pages.Dashboard.MonthlySectionReport;

namespace TicketSystem.Services;

public class OrderService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IHubContext<NotificationHub> _hubContext;

    public OrderService(IDbContextFactory<ApplicationDbContext> contextFactory, IHubContext<NotificationHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
    }

    public async Task<List<Order>> GetAllAsync(int sectionId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // جلب اسم القسم الحالي بناءً على الـ ID
        var sectionName = await context.Sections
            .Where(s => s.Id == sectionId)
            .Select(s => s.Name)
            .FirstOrDefaultAsync();

        // جلب الطلبات التي تتعلق بالقسم الحالي
        var orders = await context.Orders
            .Where(o =>
                o.SectionId == sectionId ||
                o.LastDepartment == sectionName ||
                o.OrderDetails.Any(d => d.FromDep == sectionName || d.ToDepartment == sectionName)
                //|| o.ExternalDetail != null // <-- إضافة الطلب الخارجي
                )
            .Include(o => o.Fixed)
            .Include(o => o.Section)
            .Include(o => o.OrderDetails)
                .Include(o => o.ExternalDetail) // <-- تحميل البيانات الخارجية

            .OrderByDescending(o => o.DateTime)
            .ToListAsync();

        // إضافة علامة لكل تفصيلة توضح إذا كانت مرتبطة بالقسم الحالي
        foreach (var order in orders)
        {
            foreach (var detail in order.OrderDetails)
            {
                detail.IsRelatedToCurrentSection =
                    detail.FromDep == sectionName || detail.ToDepartment == sectionName;
            }
        }

        return orders;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Orders
            .Include(o => o.Fixed)              // بيانات الطلب الثابت
            .Include(o => o.Section)            // القسم المرتبط بالطلب
            .Include(o => o.OrderDetails)       // كل تفصيلات الطلب
            .Include(o => o.ExternalDetail)     // البيانات الخارجية للطلب
            .FirstOrDefaultAsync(o => o.Id == id);
    }



    public async Task AddAsync(Order order, OrderDetailOut? externalOrder = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // حفظ بيانات الطلب الخارجي إذا وجدت
        if (externalOrder != null)
        {
            externalOrder.OrderId = order.Id;
            context.OrderDetailOuts.Add(externalOrder);
            await context.SaveChangesAsync();
        }

        // إشعارات أول تفصيلة كما في السابق
        var firstDetail = order.OrderDetails.FirstOrDefault();
        if (firstDetail != null)
        {
            var targetSection = await context.Sections
                .FirstOrDefaultAsync(s => s.Name == firstDetail.ToDepartment);

            if (targetSection != null)
            {
                var notification = new Notification
                {
                    Title = "طلب جديد محال إليكم",
                    Message = $"تم إحالة طلب جديد رقم {order.Id} إلى قسم {targetSection.Name}",
                    TargetSectionId = targetSection.Id.ToString(),
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    RelatedOrderId = order.Id
                };

                context.Notifications.Add(notification);
                await context.SaveChangesAsync();
            }
        }
    }


    public async Task UpdateAsync(Order order)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var existingOrder = await context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        if (existingOrder is null) return;

        context.Entry(existingOrder).CurrentValues.SetValues(order);

        // Remove deleted details
        foreach (var existingDetail in existingOrder.OrderDetails.ToList())
        {
            if (!order.OrderDetails.Any(d => d.Id == existingDetail.Id))
            {
                context.OrderDetails.Remove(existingDetail);
            }
        }

        // Add or update details
        foreach (var detail in order.OrderDetails)
        {
            var existingDetail = existingOrder.OrderDetails.FirstOrDefault(d => d.Id == detail.Id);
            if (existingDetail != null)
            {
                context.Entry(existingDetail).CurrentValues.SetValues(detail);
            }
            else
            {
                existingOrder.OrderDetails.Add(detail);
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var order = await context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id);
        if (order != null)
        {
            // حذف المرفقات أيضًا لو لزم الأمر
            foreach (var detail in order.OrderDetails)
            {
                if (!string.IsNullOrWhiteSpace(detail.Attachment))
                {
                    var files = detail.Attachment.Split(',');
                    foreach (var file in files)
                    {
                        var path = Path.Combine("wwwroot", "uploads", file);
                        if (File.Exists(path)) File.Delete(path);
                    }
                }
            }

            context.Orders.Remove(order);
            await context.SaveChangesAsync();
        }
    }

    public async Task<OrderDetail> AddDetailAsync(OrderDetail detail)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // إضافة التفصيلة
        context.OrderDetails.Add(detail);

        // تحديث LastDepartment في الطلب الرئيسي
        var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == detail.OrderId);
        if (order != null)
        {
            order.LastDepartment = detail.ToDepartment;
        }

        // إضافة إشعار للقسم المُحال إليه
        var section = await context.Sections
            .FirstOrDefaultAsync(s => s.Name == detail.ToDepartment);

        if (section != null)
        {
            var notification = new Notification
            {
                Title = "تفصيلة جديدة",
                Message = $"تمت إحالة الطلب رقم {detail.OrderId} إلى قسم {section.Name}",
                TargetSectionId = section.Id.ToString(),
                CreatedAt = DateTime.Now,
                IsRead = false,
                RelatedOrderId = detail.OrderId // ✅ الربط برقم الطلب
            };


            context.Notifications.Add(notification);
        }

        await context.SaveChangesAsync();

        return detail;
    }

    public async Task CloseOrderAsync(int orderId, string closerSectionName)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var order = await context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new Exception("الطلب غير موجود");

        // ✅ إغلاق الطلب
        order.Closed = true;
        //order.DateTime = DateTime.Now;

        var fixedId = order.FixedId;
        var repeateFactor = order.RepeateFactor;

        // ✅ احضر الأقسام والنقاط المرتبطة بالخدمة
        var relatedRates = await context.Rates
            .Where(r => r.OrderFixedId == fixedId)
            .ToListAsync();

        foreach (var rate in relatedRates)
        {
            var departmentId = rate.FromDepartment;

            // احسب الوقت والنقاط
            var timeMinuteRate = repeateFactor * rate.TimeMinute;
            var totalRate = repeateFactor * rate.RateValue * rate.TimeMinute;

            // 👇 احفظ البيانات في جدول RawsDatas أو أي جدول تستخدمه لتسجيل النقاط
            var rateRecord = new RawsData
            {
                OrderId = order.Id,
                OrderFixedId = (int)order.FixedId,
                RateId = (int)rate.RateValue,                // 0 أو أي قيمة افتراضية أخرى
                FromDepartmentRate = departmentId,
                TimeMinuteRate = rate.TimeMinute,
                DateTimeStart = order.DateTime ?? DateTime.Now,
                DateTimeClosed = DateTime.Now,
                ClosedOrder = true,
                RepeateFactor = (int)order.RepeateFactor,
                TotalRate = (int)rate.TimeMinute * (int)order.RepeateFactor * (int)rate.RateValue
            };

            context.RawsDatas.Add(rateRecord);
        }

        await context.SaveChangesAsync();
    }

    // دالة مساعدة لإضافة نقاط لأي قسم
    private async Task AddSectionPointsAsync(ApplicationDbContext context, string sectionName, Order order, string messagePrefix = "تم تسجيل نقاط الطلب")
    {
        var rate = await context.Rates
            .FirstOrDefaultAsync(r => r.OrderFixedId == order.Fixed!.Id && r.FromDepartment == sectionName);

        if (rate == null) return;

        int totalRate = (int)(rate.TimeMinute * rate.RateValue * (order.RepeateFactor ?? 1));

        context.RawsDatas.Add(new RawsData
        {
            OrderId = order.Id,
            OrderFixedId = order.Fixed.Id,
            RateId = rate.ID,
            FromDepartmentRate = rate.FromDepartment,
            TimeMinuteRate = rate.TimeMinute,
            DateTimeStart = order.DateTime ?? DateTime.Now,
            DateTimeClosed = DateTime.Now,
            ClosedOrder = true,
            RepeateFactor = order.RepeateFactor ?? 1,
            TotalRate = totalRate
        });

        await SendSectionNotificationAsync(context, sectionName, order.Id, $"{messagePrefix} ({sectionName}).");
    }

    // دالة مساعدة لإرسال الإشعار عبر SignalR
    private async Task SendSectionNotificationAsync(ApplicationDbContext context, string sectionName, int orderId, string message)
    {
        var sectionEntity = await context.Sections.FirstOrDefaultAsync(s => s.Name == sectionName);
        if (sectionEntity != null)
        {
            var notification = new Notification
            {
                Title = "تم تسجيل نقاط الطلب",
                Message = message,
                TargetSectionId = sectionEntity.Id.ToString(),
                CreatedAt = DateTime.Now,
                IsRead = false,
                RelatedOrderId = orderId
            };
            context.Notifications.Add(notification);

            await _hubContext.Clients.Group(sectionEntity.Id.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    notification.Title,
                    notification.Message,
                    notification.RelatedOrderId,
                    notification.CreatedAt
                });
        }
    }


    public async Task<List<SectionDayData>> GetSectionMonthlyDataAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var month = DateTime.Now.Month;
        var year = DateTime.Now.Year;

        var rawData = await context.RawsDatas
            .Where(r => r.DateTimeClosed.Month == month && r.DateTimeClosed.Year == year)
            .GroupBy(r => new { r.FromDepartmentRate, Day = r.DateTimeClosed.Day })
            .Select(g => new SectionDayData
            {
                SectionName = g.Key.FromDepartmentRate,
                Day = g.Key.Day,
                Total = g.Sum(x => x.TotalRate)
            })
            .ToListAsync();

        return rawData;
    }
    public class SectionDayData
    {
        public string SectionName { get; set; } = "";
        public int Day { get; set; }
        public int Total { get; set; }
    }
    public async Task<(List<Order>, int)> GetPagedAsync(int sectionId, int pageIndex, int pageSize, string status = "all")
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var sectionName = await context.Sections
            .Where(s => s.Id == sectionId)
            .Select(s => s.Name)
            .FirstOrDefaultAsync();

        // نستخدم IQueryable من البداية لتجنب مشاكل النوع
        IQueryable<Order> query = context.Orders
            .Where(o =>
                o.SectionId == sectionId ||
                o.LastDepartment == sectionName ||
                o.OrderDetails.Any(d => d.FromDep == sectionName || d.ToDepartment == sectionName))
            // || o.ExternalDetail != null)
            .Include(o => o.Section)
            .Include(o => o.Fixed)
            .Include(o => o.OrderDetails)
            .Include(o => o.ExternalDetail); // تحميل البيانات الخارجية

        // تطبيق الفلترة حسب الحالة
        query = status switch
        {
            "open" => query.Where(o => o.Closed != true),
            "closed" => query.Where(o => o.Closed == true),
            _ => query
        };

        int totalCount = await query.CountAsync();

        var orders = await query
            .OrderByDescending(o => o.DateTime)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (orders, totalCount);
    }

    // لجلب تفاصيل الطلب عند الحاجة
    public async Task<List<OrderDetail>> GetOrderDetailsAsync(int orderId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.OrderDetails
            .Where(d => d.OrderId == orderId)
            .ToListAsync();
    }
    // داخل OrderService
    public async Task<List<OrderDetail>> GetAllOrderDetailsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.OrderDetails
            .Include(d => d.Order) // لو تحب تجلب بيانات الطلب الرئيسية
            .ToListAsync();
    }










    // داخل OrderService.cs
    public async Task<List<OrderDetailExtended>> GetAllOrderDetailsExtendedAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // جلب تفاصيل الطلبات مع البيانات المرتبطة
        var details = await context.OrderDetails
            .Include(d => d.Order)
                .ThenInclude(o => o.Section) // القسم الذي أنشأ الطلب
            .ToListAsync();

        return details.Select(d => new OrderDetailExtended
        {
            OrderId = d.OrderId,
            ServiceName = d.ToDepartment,
            CreatedBySection = d.FromDep,
            ClosedBySection = d.Order?.LastDepartment ?? "",
            CreationDate = d.CreationDate,
        }).ToList();
    }

    // كلاس جديد لتعريف البيانات الممتدة للعرض
    public class OrderDetailExtended
    {
        public int OrderId { get; set; }
        public string ServiceName { get; set; } = "";
        public string CreatedBySection { get; set; } = "";
        public string ClosedBySection { get; set; } = "";
        public DateTime CreationDate { get; set; }
        public DateTime? ClosedDate { get; set; }
    }
    public async Task<List<OrderDetailExtended>> GetOrderDetailsByDateAsync(DateTime? from = null, DateTime? to = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.OrderDetails.Include(d => d.Order).ThenInclude(o => o.Fixed).AsQueryable();

        if (from.HasValue)
            query = query.Where(d => d.CreationDate >= from.Value);

        if (to.HasValue)
            query = query.Where(d => d.CreationDate <= to.Value);

        var details = await query.ToListAsync();

        return details.Select(d => new OrderDetailExtended
        {
            OrderId = d.OrderId,
            ServiceName = d.Order?.Fixed?.DisplayText ?? "-", // حماية من null
            CreatedBySection = d.FromDep,
            ClosedBySection = d.Order?.LastDepartment ?? "",
            CreationDate = d.CreationDate,
            ClosedDate = d.Order?.Closed == true ? d.Order?.DateTime : null
        }).ToList();
    }

    public async Task<List<ServiceUsage>> GetServiceUsageAsync(DateTime from, DateTime to)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // جلب البيانات أولاً من قاعدة البيانات
        var details = await context.OrderDetails
            .Include(d => d.Order)
                .ThenInclude(o => o.Fixed)
            .Where(d => d.CreationDate >= from && d.CreationDate <= to)
            .ToListAsync(); // تحميل النتائج أولاً

        // الآن يمكن الوصول للخاصية DisplayText على الجانب العميل
        var usage = details
            .GroupBy(d => d.Order?.Fixed?.DisplayText ?? "-")
            .Select(g => new ServiceUsage
            {
                ServiceName = g.Key,
                UsageCount = g.Count()
            })
            .ToList();

        return usage;
    }
    // موجود في نفس الكلاس OrderService
    public async Task<List<SectionDayData>> GetSectionMonthlyDataAsync(int month, int year)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var rawData = await context.RawsDatas
            .Where(r => r.DateTimeClosed.Month == month && r.DateTimeClosed.Year == year)
            .GroupBy(r => new { r.FromDepartmentRate, Day = r.DateTimeClosed.Day })
            .Select(g => new SectionDayData
            {
                SectionName = g.Key.FromDepartmentRate,
                Day = g.Key.Day,
                Total = g.Sum(x => x.TotalRate)
            })
            .ToListAsync();

        return rawData;
    }



    public class ServiceUsage
    {
        public string ServiceName { get; set; } = "";
        public int UsageCount { get; set; }
    }

}