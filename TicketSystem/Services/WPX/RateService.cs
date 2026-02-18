namespace TicketSystem.Services.WPX
{
    using Microsoft.EntityFrameworkCore;
    using TicketSystem.Data;
    using TicketSystem.TSModel;

    public class RateService
    {
        private readonly ApplicationDbContext _context;

        public RateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Rate>> GetAllAsync()
        {
            return await _context.Rates
                .Include(r => r.OrderFixed)
                .ToListAsync();
        }

        public async Task<Rate?> GetByIdAsync(int id)
        {
            return await _context.Rates
                .Include(r => r.OrderFixed)
                .FirstOrDefaultAsync(r => r.ID == id);
        }

        public async Task AddAsync(Rate rate)
        {
            _context.Rates.Add(rate);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Rate rate)
        {
            var existingRate = await _context.Rates.FindAsync(rate.ID);
            if (existingRate != null)
            {
                existingRate.OrderFixedId = rate.OrderFixedId;
                existingRate.FromDepartment = rate.FromDepartment;
                existingRate.TimeMinute = rate.TimeMinute;
                existingRate.RateValue = rate.RateValue;

                await _context.SaveChangesAsync();
            }
        }


        public async Task DeleteAsync(int id)
        {
            var rate = await _context.Rates.FindAsync(id);
            if (rate != null)
            {
                _context.Rates.Remove(rate);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int? orderFixedId, string fromDepartment, int excludeId = 0)
        {
            return await _context.Rates
                .AnyAsync(r => r.OrderFixedId == orderFixedId
                               && r.FromDepartment == fromDepartment
                               && r.ID != excludeId);
        }


    }

}
