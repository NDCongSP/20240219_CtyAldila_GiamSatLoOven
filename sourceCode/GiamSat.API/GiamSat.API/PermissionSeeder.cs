using GiamSat.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;

namespace GiamSat.API
{
    public static class PermissionSeeder
    {
        private static readonly (string Module, string Action, string Description)[] PermissionDefs =
        {
            // OVEN MODULE
            ("Oven_Home", "View", "Truy cập và xem Dashboard Oven"),
            ("Oven_Config", "View", "Xem trang Cấu hình Oven"),
            ("Oven_Config", "Create", "Thêm mới cấu hình Oven"),
            ("Oven_Config", "Edit", "Chỉnh sửa cấu hình Oven"),
            ("Oven_Config", "Delete", "Xóa cấu hình Oven"),
            ("Oven_Report", "View", "Xem báo cáo Oven"),
            ("Oven_Report", "Export", "Xuất file báo cáo Oven"),
            ("Oven_Settings", "View", "Xem cài đặt hệ thống Oven"),
            ("Oven_Settings", "Edit", "Chỉnh sửa cài đặt Oven"),

            // REVO MODULE
            ("Revo_Home", "View", "Truy cập và xem Dashboard Revo"),
            ("Revo_Config", "View", "Xem trang Cấu hình Revo"),
            ("Revo_Config", "Create", "Thêm cấu hình Revo"),
            ("Revo_Config", "Edit", "Sửa cấu hình Revo"),
            ("Revo_Config", "Delete", "Xóa cấu hình Revo"),
            ("Revo_Report", "View", "Xem báo cáo Revo"),
            ("Revo_Report", "Export", "Xuất file báo cáo Revo")
        };

        public static async Task SeedAsync(ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager)
        {
            // 1. Ensure default roles exist
            foreach (var roleName in UserRoles.DefaultRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // 2. Seed Permissions table
            foreach (var item in PermissionDefs)
            {
                if (!await dbContext.Permissions.AnyAsync(x => x.Module == item.Module && x.Actions == item.Action))
                {
                    dbContext.Permissions.Add(new Permissions
                    {
                        Id = Guid.NewGuid(),
                        Name = item.Description,
                        Module = item.Module,
                        Actions = item.Action,
                        Description = item.Description,
                        IsActived = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await dbContext.SaveChangesAsync();

            // 3. Grant all permissions to Admin role — ONLY on first run (when Admin has zero permissions)
            var adminRole = await roleManager.FindByNameAsync(UserRoles.Admin);
            if (adminRole == null) return;

            var adminHasAny = await dbContext.RoleToPermissions.AnyAsync(x => x.RoleId == adminRole.Id);
            if (adminHasAny) return; // Admin already has permissions configured — don't override manual changes

            var allPermissions = await dbContext.Permissions.ToListAsync();
            foreach (var perm in allPermissions)
            {
                dbContext.RoleToPermissions.Add(new RoleToPermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = adminRole.Id,
                    RoleName = adminRole.Name,
                    PermissionId = perm.Id,
                    PermisionName = perm.Name,
                    PermisionDescription = perm.Description,
                    IsActived = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
