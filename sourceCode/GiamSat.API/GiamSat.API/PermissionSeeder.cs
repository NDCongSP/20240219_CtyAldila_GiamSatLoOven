using GiamSat.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.API
{
    public static class PermissionSeeder
    {
        private static readonly (string Code, string Module, string Action, string Description)[] Permissions =
        {
            (PermissionNames.Revo.View, "Revo", "View", "View Revo data"),
            (PermissionNames.Revo.Create, "Revo", "Create", "Create Revo data"),
            (PermissionNames.Revo.Edit, "Revo", "Edit", "Edit Revo data"),
            (PermissionNames.Revo.Delete, "Revo", "Delete", "Delete Revo data"),
            (PermissionNames.Revo.Export, "Revo", "Export", "Export Revo data"),
            (PermissionNames.Revo.Approve, "Revo", "Approve", "Approve Revo data"),
            (PermissionNames.Oven.View, "Oven", "View", "View Oven data"),
            (PermissionNames.Oven.Create, "Oven", "Create", "Create Oven data"),
            (PermissionNames.Oven.Edit, "Oven", "Edit", "Edit Oven data"),
            (PermissionNames.Oven.Delete, "Oven", "Delete", "Delete Oven data"),
            (PermissionNames.Oven.Export, "Oven", "Export", "Export Oven data"),
            (PermissionNames.Oven.Approve, "Oven", "Approve", "Approve Oven data"),
        };

        public static async Task SeedAsync(ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in UserRoles.DefaultRoles)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            foreach (var item in Permissions)
            {
                var exists = await dbContext.SecurityPermissions.AnyAsync(x => x.Code == item.Code);
                if (!exists)
                {
                    dbContext.SecurityPermissions.Add(new SecurityPermission
                    {
                        Code = item.Code,
                        Module = item.Module,
                        Action = item.Action,
                        Description = item.Description
                    });
                }
            }

            await dbContext.SaveChangesAsync();

            var adminRole = await roleManager.FindByNameAsync(UserRoles.Admin);
            if (adminRole == null) return;
            var allPermissions = await dbContext.SecurityPermissions.ToListAsync();
            foreach (var permission in allPermissions)
            {
                var mapExists = await dbContext.RolePermissions.AnyAsync(x => x.RoleId == adminRole.Id && x.PermissionId == permission.Id);
                if (!mapExists)
                {
                    dbContext.RolePermissions.Add(new RolePermission
                    {
                        RoleId = adminRole.Id,
                        PermissionId = permission.Id
                    });
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
