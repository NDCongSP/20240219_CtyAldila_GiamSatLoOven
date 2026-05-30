using GiamSat.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using System.Linq;

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
            ("Revo_Report", "Export", "Xuất file báo cáo Revo"),

            // SYSTEM MODULE
            ("System_Config", "View", "Xem cấu hình hệ thống"),
            ("System_Config", "Edit", "Chỉnh sửa cấu hình hệ thống"),

            // SYSTEM LOGS
            ("System_Logs", "View", "Xem log hệ thống (Serilog)"),

            // TEMPERATURE MODULE
            ("Temperature_Home", "View", "Truy cập và xem Dashboard Nhiệt độ"),
            ("Temperature_Config", "View", "Xem danh sách Cấu hình Nhiệt độ"),
            ("Temperature_Config", "Create", "Thêm Cấu hình Nhiệt độ"),
            ("Temperature_Config", "Edit", "Sửa Cấu hình Nhiệt độ"),
            ("Temperature_Config", "Delete", "Xóa Cấu hình Nhiệt độ"),
            ("Temperature_Report", "View", "Xem báo cáo Nhiệt độ"),
            ("Temperature_Report", "Export", "Xuất file báo cáo Nhiệt độ"),

            // SANDING MODULE
            ("Sanding_Config", "View", "Xem trang Cấu hình Sanding"),
            ("Sanding_Config", "Create", "Thêm cấu hình Sanding"),
            ("Sanding_Config", "Edit", "Sửa cấu hình Sanding"),
            ("Sanding_Config", "Delete", "Xóa cấu hình Sanding"),
            ("Sanding_Report", "View", "Xem báo cáo Sanding")
        };

        public static async Task SeedAsync(ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager)
        {
            // Idempotent — chạy lại sẽ chỉ thêm permission còn thiếu (vd System_Logs.View khi nâng cấp).

            // 1. Ensure default roles exist
            foreach (var roleName in UserRoles.DefaultRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // 2. Seed Permissions table (Additive: only add what is missing)
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

            // 3. Grant missing permissions to Admin role
            var adminRole = await roleManager.FindByNameAsync(UserRoles.Admin);
            if (adminRole == null) return;

            var existingAdminPerms = await dbContext.RoleToPermissions
                .Where(x => x.RoleId == adminRole.Id)
                .Select(x => x.PermissionId)
                .ToListAsync();

            var allPermissions = await dbContext.Permissions.ToListAsync();
            var missingForAdmin = allPermissions.Where(p => !existingAdminPerms.Contains(p.Id)).ToList();

            foreach (var perm in missingForAdmin)
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
