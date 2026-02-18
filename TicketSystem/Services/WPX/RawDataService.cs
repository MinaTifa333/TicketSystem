namespace TicketSystem.Services.WPX
{
    using global::TicketSystem.Data;
    using Microsoft.EntityFrameworkCore;
    using TicketSystem.Data;
    using TicketSystem.TSModel;

    
        public class RawDataService
        {
            private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

            public RawDataService(IDbContextFactory<ApplicationDbContext> contextFactory)
            {
                _contextFactory = contextFactory;
            }

            public async Task<List<PointsSummary>> GetPointsBySectionAsync()
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                return await context.RawsDatas
                    .GroupBy(r => r.FromDepartmentRate)
                    .Select(g => new PointsSummary
                    {
                        SectionName = g.Key,
                        TotalPoints = g.Sum(x => x.TotalRate),
                        OrdersCount = g.Select(x => x.OrderId).Distinct().Count()
                    })
                    .OrderByDescending(x => x.TotalPoints)
                    .ToListAsync();
            }
        public async Task<List<RawsData>> GetAllRawDataAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.RawsDatas
                .Include(r => r.Order)
                .Include(r => r.OrderFixed)
                .Include(r => r.Rate)
                .OrderByDescending(r => r.DateTimeClosed)
                .ToListAsync();
        }




    }

    public class PointsSummary
        {
            public string SectionName { get; set; } = string.Empty;
            public int TotalPoints { get; set; }
            public int OrdersCount { get; set; }
        }
    

}
