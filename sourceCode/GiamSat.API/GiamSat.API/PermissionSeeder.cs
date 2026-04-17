using GiamSat.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;

namespace GiamSat.API
{
    public static class PermissionSeeder
    {
        private static readonly (string Code, string Module, string Action, string Description)[] PermissionDefs =
        {
            (PermissionNames.Revo.View,    "Revo", "View",    "View Revo data"),
            (PermissionNames.Revo.Create,  "Revo", "Create",  "Create Revo data"),
            (PermissionNames.Revo.Edit,    "Revo", "Edit",    "Edit Revo data"),
            (PermissionNames.Revo.Delete,  "Revo", "Delete",  "Delete Revo data"),
            (PermissionNames.Revo.Export,  "Revo", "Export",  "Export Revo data"),
            (PermissionNames.Revo.Approve, "Revo", "Approve", "Approve Revo data"),
            (PermissionNames.Oven.View,    "Oven", "View",    "View Oven data"),
            (PermissionNames.Oven.Create,  "Oven", "Create",  "Create Oven data"),
            (PermissionNames.Oven.Edit,    "Oven", "Edit",    "Edit Oven data"),
            (PermissionNames.Oven.Delete,  "Oven", "Delete",  "Delete Oven data"),
            (PermissionNames.Oven.Export,  "Oven", "Export",  "Export Oven data"),
            (PermissionNames.Oven.Approve, "Oven", "Approve", "Approve Oven data"),
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

            // 3. Grant all permissions to Admin role
            var adminRole = await roleManager.FindByNameAsync(UserRoles.Admin);
            if (adminRole == null) return;

            var allPermissions = await dbContext.Permissions.ToListAsync();
            foreach (var perm in allPermissions)
            {
                if (!await dbContext.RoleToPermissions.AnyAsync(x => x.RoleId == adminRole.Id && x.PermissionId == perm.Id))
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
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
