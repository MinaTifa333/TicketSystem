using TicketSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class RoleService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleService(RoleManager<IdentityRole> roleManager, IServiceScopeFactory scopeFactory)
    {
        _roleManager = roleManager;
        _scopeFactory = scopeFactory;
    }

    // الحصول على كل الأدوار
    public async Task<List<IdentityRole>> GetAllRolesAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Roles.AsNoTracking().ToListAsync();
    }

    // إضافة دور جديد
    public async Task<IdentityResult> AddRoleAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            return await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
        return IdentityResult.Failed(new IdentityError { Description = "الدور موجود بالفعل" });
    }

    // حذف دور
    public async Task<IdentityResult> DeleteRoleAsync(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role != null)
        {
            return await _roleManager.DeleteAsync(role);
        }
        return IdentityResult.Failed(new IdentityError { Description = "الدور غير موجود" });
    }

    // الحصول على جميع الصلاحيات المرتبطة برول معين
    public async Task<List<string>> GetPermissionsByRoleAsync(string roleName)
    {
        using var scope = _scopeFactory.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null) return new();

        var claims = await roleManager.GetClaimsAsync(role);
        return claims
            .Where(c => c.Type.StartsWith("Permissions."))
            .Select(c => c.Type)
            .ToList();
    }


    // إضافة صلاحية إلى دور
    public async Task AddPermissionToRoleAsync(string roleName, string permission)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null) return;

        var existingClaims = await _roleManager.GetClaimsAsync(role);
        if (!existingClaims.Any(c => c.Type == permission))
        {
            await _roleManager.AddClaimAsync(role, new Claim(permission, "true"));
        }
    }

    // تحديث الصلاحيات لدور
    public async Task UpdateRolePermissionsAsync(string roleId, List<string> newPermissions)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null) return;

        var existingClaims = await _roleManager.GetClaimsAsync(role);

        // إزالة كل الصلاحيات القديمة
        foreach (var claim in existingClaims.Where(c => c.Type.StartsWith("Permissions.")))
        {
            await _roleManager.RemoveClaimAsync(role, claim);
        }

        // إضافة الصلاحيات الجديدة
        foreach (var permission in newPermissions)
        {
            await _roleManager.AddClaimAsync(role, new Claim(permission, "true"));
        }
    }
}
