    using Microsoft.EntityFrameworkCore;
using TicketSystem.Data;
using TicketSystem.TSModel;

namespace TicketSystem.Services
{


    public class OrderFixedService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public OrderFixedService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<OrderFixed>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.OrderFixeds
                    .AsNoTracking()
                .OrderByDescending(x => x.CreationDate)

                .ToListAsync();
        }

        public async Task AddAsync(OrderFixed item)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            item.CreationDate = DateTime.Now;
            context.OrderFixeds.Add(item);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(OrderFixed item)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var existing = await context.OrderFixeds.FindAsync(item.Id);
            if (existing is null) return;

            existing.Types = item.Types;
            existing.Level1 = item.Level1;
            existing.Level2 = item.Level2;
            existing.Level3 = item.Level3;
            existing.Level4 = item.Level4;
            existing.Level5 = item.Level5;
            existing.Level6 = item.Level6;
            existing.Label = item.Label;
            existing.WorkFlow = item.WorkFlow;
            existing.Okm = item.Okm;
            existing.CreatedBy = item.CreatedBy;

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var item = await context.OrderFixeds.FindAsync(id);
            if (item != null)
            {
                context.OrderFixeds.Remove(item);
                await context.SaveChangesAsync();
            }
        }
    }

}
