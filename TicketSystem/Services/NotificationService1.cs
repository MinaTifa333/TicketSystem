// ✅ NotificationService.cs
using Microsoft.EntityFrameworkCore;
using TicketSystem.Data;
using TicketSystem.Model;

namespace TicketSystem.Services
{
    public class NotificationService1
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public NotificationService1(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Notification>> GetAllBySectionAsync(int sectionId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Notifications
                .Where(n => n.TargetSectionId == sectionId.ToString())
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetUnreadBySectionAsync(int sectionId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Notifications
                .Where(n => n.TargetSectionId == sectionId.ToString() && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(string title, string message, int sectionId, int? relatedOrderId = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var notification = new Notification
            {
                Title = title,
                Message = message,
                TargetSectionId = sectionId.ToString(),
                CreatedAt = DateTime.Now,
                IsRead = false,
                RelatedOrderId = relatedOrderId
            };
            context.Notifications.Add(notification);
            await context.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var noti = await context.Notifications.FindAsync(id);
            if (noti != null && !noti.IsRead)
            {
                noti.IsRead = true;
                await context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int sectionId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var notifications = await context.Notifications
                .Where(n => n.TargetSectionId == sectionId.ToString() && !n.IsRead)
                .ToListAsync();

            foreach (var noti in notifications)
            {
                noti.IsRead = true;
            }

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var noti = await context.Notifications.FindAsync(id);
            if (noti != null)
            {
                context.Notifications.Remove(noti);
                await context.SaveChangesAsync();
            }
        }
    }
}
