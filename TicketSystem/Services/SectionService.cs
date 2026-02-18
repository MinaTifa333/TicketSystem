using Microsoft.EntityFrameworkCore;
using TicketSystem.Data;
using TicketSystem.TSModel;

namespace TicketSystem.Services
{
    public class SectionService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public SectionService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Section>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Sections.ToListAsync();
        }

        public async Task<Section?> GetByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Sections.FindAsync(id);
        }

        public async Task AddAsync(Section section)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Sections.Add(section);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Section section)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Sections.Update(section);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var section = await context.Sections.FindAsync(id);
            if (section is not null)
            {
                context.Sections.Remove(section);
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> NameExistsAsync(string name)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Sections
                .AnyAsync(s => EF.Functions.Collate(s.Name, "Arabic_CI_AS") == name);
        }

        public async Task<bool> IsNameUsedByAnotherAsync(int id, string name)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Sections
                .AnyAsync(s => s.Id != id && s.Name == name);
        }

        public async Task<List<Section>> GetSectionsForUserAsync(string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.UserSections
                                .Where(us => us.UserId == userId)
                                .Select(us => us.Section)
                                .ToListAsync();
        }

    }
}
