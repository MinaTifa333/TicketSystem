using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicketSystem.Data;
using TicketSystem.Model;

namespace TicketSystem.Services
{
    public class UserContextService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthenticationStateProvider _authProvider;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public UserContextService(
            UserManager<ApplicationUser> userManager,
            AuthenticationStateProvider authProvider,
            IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _userManager = userManager;
            _authProvider = authProvider;
            _contextFactory = contextFactory;
        }

        // ✅ استرجاع المستخدم الحالي
        public async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            var authState = await _authProvider.GetAuthenticationStateAsync();

            // لإنهاء المشكلة: لا تستخدم _userManager مباشرة
            var userId = authState.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
                return null;

            // استخدم DbContextFactory بدلاً من UserManager
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users
                .Include(u => u.UserSections)
                .ThenInclude(us => us.Section)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }


        // ✅ استرجاع معرف المستخدم
        public async Task<string?> GetUserIdAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.Id;
        }

        // ✅ استرجاع اسم المستخدم
        public async Task<string?> GetUserNameAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.UserName;
        }

        // ✅ استرجاع الأدوار
        public async Task<List<string>> GetUserRolesAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return new List<string>();
            return (await _userManager.GetRolesAsync(user)).ToList();
        }

        // ✅ التحقق من كون المستخدم ضمن دور معين
        public async Task<bool> IsInRoleAsync(string role)
        {
            var user = await GetCurrentUserAsync();
            return user != null && await _userManager.IsInRoleAsync(user, role);
        }

        // ✅ استرجاع الأقسام المرتبطة بالمستخدم
        public async Task<List<int>> GetUserSectionIdsAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return new List<int>();

            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.UserSections
                .Where(us => us.UserId == user.Id)
                .Select(us => us.SectionId)
                .ToListAsync();
        }

        // ✅ استرجاع أول قسم (افتراضيًا)
        public async Task<int?> GetFirstSectionIdAsync()
        {
            var sectionIds = await GetUserSectionIdsAsync();
            return sectionIds.FirstOrDefault();
        }
    }
}
