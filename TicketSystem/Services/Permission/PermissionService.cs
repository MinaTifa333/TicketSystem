using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicketSystem.Data;

public class PermissionService
{
    private readonly IServiceProvider _serviceProvider;

    public PermissionService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    // جلب جميع صلاحيات المستخدم
    public async Task<List<string>> GetUserPermissionsAsync(ApplicationUser user)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // جلب Claims للمستخدم
        var claims = await db.UserClaims
                             .Where(c => c.UserId == user.Id)
                             .Select(c => c.ClaimValue)
                             .ToListAsync();

        // جلب Roles للمستخدم
        var userRoles = await db.UserRoles
                                .Where(ur => ur.UserId == user.Id)
                                .Join(db.Roles,
                                      ur => ur.RoleId,
                                      r => r.Id,
                                      (ur, r) => r.Name)
                                .ToListAsync();

        // جلب كل صلاحيات Roles
        var roleClaims = new List<string>();
        foreach (var roleName in userRoles)
        {
            var role = await scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>()
                                                .FindByNameAsync(roleName);
            if (role != null)
            {
                var claimsForRole = await scope.ServiceProvider
                                               .GetRequiredService<RoleManager<IdentityRole>>()
                                               .GetClaimsAsync(role);
                roleClaims.AddRange(claimsForRole.Select(c => c.Type));
            }
        }

        // دمج كل الصلاحيات
        var permissions = claims.Concat(roleClaims).ToList();
        return permissions;
    }

    // التحقق من كون المستخدم Admin بدون مشاكل DbContext
    public async Task<bool> IsUserAdminAsync(ApplicationUser user)
    {
        var permissions = await GetUserPermissionsAsync(user);
        return permissions.Contains("Admin");
    }
}
