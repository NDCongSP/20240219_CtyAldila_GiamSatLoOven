using GiamSat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = UserRoles.Admin)]
    public class PermissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermissionsController(
            ApplicationDbContext dbContext,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET api/permissions/roles
        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<IdentityRoleDto>>> GetRoles()
        {
            var roles = await _roleManager.Roles.OrderBy(x => x.Name).ToListAsync();

            var userRoleCounts = await _dbContext.UserRoles
                .GroupBy(x => x.RoleId)
                .Select(x => new { RoleId = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.RoleId, x => x.Count);

            var rolesPerms = await _dbContext.RoleToPermissions.ToListAsync();
            var perms = await _dbContext.Permissions.Where(p => p.IsActived == true).ToDictionaryAsync(x => x.Id);

            var roleClaims = rolesPerms
                .Where(x => perms.ContainsKey(x.PermissionId))
                .GroupBy(x => x.RoleId)
                .Select(x => new
                {
                    RoleId = x.Key,
                    Claims = x.Select(y => perms[y.PermissionId].Module + "." + perms[y.PermissionId].Actions).Distinct().ToList()
                })
                .ToDictionary(x => x.RoleId, x => x.Claims);

            var result = roles.Select(r => new IdentityRoleDto
            {
                Id = r.Id,
                Name = r.Name ?? string.Empty,
                UserCount = userRoleCounts.TryGetValue(r.Id, out var c) ? c : 0,
                Claims = roleClaims.TryGetValue(r.Id, out var claims) ? claims : new List<string>()
            }).ToList();

            return Ok(result);
        }

        // POST api/permissions/roles
        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] IdentityRoleDto model)
        {
            if (string.IsNullOrWhiteSpace(model?.Name))
                return BadRequest(new { Message = "Role name is required." });

            if (await _roleManager.RoleExistsAsync(model.Name))
                return BadRequest(new { Message = "Role already exists." });

            var result = await _roleManager.CreateAsync(new IdentityRole(model.Name));
            if (!result.Succeeded)
                return BadRequest(new { Message = string.Join("; ", result.Errors.Select(x => x.Description)) });

            return Ok();
        }

        // PUT api/permissions/roles/{id}
        [HttpPut("roles/{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] IdentityRoleDto model)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            if (string.IsNullOrWhiteSpace(model?.Name))
                return BadRequest(new { Message = "Role name is required." });

            role.Name = model.Name;
            // Also update redundant RoleToPermissions.RoleName
            var maps = await _dbContext.RoleToPermissions.Where(x => x.RoleId == id).ToListAsync();
            foreach (var map in maps) map.RoleName = role.Name;

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
                return BadRequest(new { Message = string.Join("; ", result.Errors.Select(x => x.Description)) });

            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        // DELETE api/permissions/roles/{id}
        [HttpDelete("roles/{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            if (string.Equals(role.Name, UserRoles.Admin, System.StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { Message = "Cannot delete Admin role." });
            
            // Usage check: Ensure no users are assigned this role
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            if (usersInRole.Any())
                return BadRequest(new { Message = "Không thể xóa Role đang được gán cho người dùng." });

            // Usage check: Ensure no permissions are mapped to this role
            var hasPermissions = await _dbContext.RoleToPermissions.AnyAsync(x => x.RoleId == id);
            if (hasPermissions)
                return BadRequest(new { Message = "Không thể xóa Role đang có các quyền liên kết." });

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                return BadRequest(new { Message = string.Join("; ", result.Errors.Select(x => x.Description)) });

            var redundantMaps = await _dbContext.RoleToPermissions.Where(x => x.RoleId == id).ToListAsync();
            _dbContext.RoleToPermissions.RemoveRange(redundantMaps);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        // GET api/permissions/permissions
        [HttpGet("permissions")]
        public async Task<ActionResult<IEnumerable<Permissions>>> GetPermissions()
        {
            var permissions = await _dbContext.Permissions
                .Where(x => x.IsActived == true)
                .OrderBy(x => x.Module)
                .ThenBy(x => x.Actions)
                .ToListAsync();

            return Ok(permissions);
        }

        // POST api/permissions/permissions
        [HttpPost("permissions")]
        public async Task<IActionResult> CreatePermission([FromBody] Permissions model)
        {
            if (string.IsNullOrWhiteSpace(model?.Name) || string.IsNullOrWhiteSpace(model.Module) || string.IsNullOrWhiteSpace(model.Actions))
                return BadRequest(new { Message = "Name, Module, and Actions are required." });

            if (await _dbContext.Permissions.AnyAsync(x => x.Module == model.Module && x.Actions == model.Actions))
                return BadRequest(new { Message = "This Permission (Module + Action) already exists." });

            model.Id = Guid.NewGuid();
            model.CreatedAt = DateTime.UtcNow;
            model.IsActived ??= true;

            _dbContext.Permissions.Add(model);
            await _dbContext.SaveChangesAsync();

            return Ok(model);
        }

        // PUT api/permissions/permissions/{id}
        [HttpPut("permissions/{id}")]
        public async Task<IActionResult> UpdatePermission(Guid id, [FromBody] Permissions model)
        {
            var permission = await _dbContext.Permissions.FindAsync(id);
            if (permission == null) return NotFound();

            if (string.IsNullOrWhiteSpace(model?.Name) || string.IsNullOrWhiteSpace(model.Module) || string.IsNullOrWhiteSpace(model.Actions))
                return BadRequest(new { Message = "Name, Module, and Actions are required." });

            var duplicate = await _dbContext.Permissions.FirstOrDefaultAsync(x => x.Id != id && x.Module == model.Module && x.Actions == model.Actions);
            if (duplicate != null)
                return BadRequest(new { Message = "Another Permission with the same Module + Action already exists." });

            permission.Name = model.Name;
            permission.Module = model.Module;
            permission.Actions = model.Actions;
            permission.Description = model.Description;
            permission.IsActived = model.IsActived ?? true;

            // Update redundant properties in RoleToPermissions
            var relatedMappings = await _dbContext.RoleToPermissions.Where(x => x.PermissionId == id).ToListAsync();
            foreach (var mapping in relatedMappings)
            {
                mapping.PermisionName = permission.Name;
                mapping.PermisionDescription = permission.Description;
            }

            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        // DELETE api/permissions/permissions/{id}
        [HttpDelete("permissions/{id}")]
        public async Task<IActionResult> DeletePermission(Guid id)
        {
            var permission = await _dbContext.Permissions.FindAsync(id);
            if (permission == null) return NotFound();

            // Usage check: Ensure no roles are using this permission
            var isUsedInRoles = await _dbContext.RoleToPermissions.AnyAsync(x => x.PermissionId == id);
            if (isUsedInRoles)
                return BadRequest(new { Message = "Không thể xóa Quyền đang được gán cho các vai trò." });

            _dbContext.Permissions.Remove(permission);

            // Cascade delete orphaned mappings manually
            var relatedMappings = await _dbContext.RoleToPermissions.Where(x => x.PermissionId == id).ToListAsync();
            _dbContext.RoleToPermissions.RemoveRange(relatedMappings);

            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        // PUT api/permissions/roles/{roleId}/permissions
        [HttpPut("roles/{roleId}/permissions")]
        public async Task<IActionResult> UpdateRolePermissions(string roleId, [FromBody] List<string> permissionCodes)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return NotFound();

            permissionCodes ??= new List<string>();
            permissionCodes = permissionCodes.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();

            var currentMatches = await _dbContext.RoleToPermissions.Where(x => x.RoleId == roleId).ToListAsync();
            _dbContext.RoleToPermissions.RemoveRange(currentMatches);

            var activePerms = await _dbContext.Permissions.Where(p => p.IsActived == true).ToListAsync();
            var permsToAdd = activePerms.Where(p => permissionCodes.Contains(p.Module + "." + p.Actions)).ToList();

            foreach (var perm in permsToAdd)
            {
                _dbContext.RoleToPermissions.Add(new RoleToPermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = roleId,
                    RoleName = role.Name,
                    PermissionId = perm.Id,
                    PermisionName = perm.Name,
                    PermisionDescription = perm.Description,
                    IsActived = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        // GET api/permissions/users
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<IdentityUserDto>>> GetUsers()
        {
            var users = await _userManager.Users.OrderBy(x => x.UserName).ToListAsync();
            var allRoleMaps = await _dbContext.RoleToPermissions.ToListAsync();
            var allPerms = await _dbContext.Permissions.ToDictionaryAsync(p => p.Id);

            var result = new List<IdentityUserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var claims = allRoleMaps
                    .Where(x => x.RoleName != null && roles.Contains(x.RoleName) && allPerms.ContainsKey(x.PermissionId) && allPerms[x.PermissionId].IsActived == true)
                    .Select(x => allPerms[x.PermissionId].Module + "." + allPerms[x.PermissionId].Actions)
                    .Distinct()
                    .ToList();

                result.Add(new IdentityUserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email,
                    Roles = roles.ToList(),
                    Claims = claims
                });
            }

            return Ok(result);
        }

        // PUT api/permissions/users/{id}/roles
        [HttpPut("users/{id}/roles")]
        public async Task<IActionResult> UpdateUserRoles(string id, [FromBody] List<string> roles)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (roles != null && roles.Count > 0)
            {
                var validRoles = roles.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
                await _userManager.AddToRolesAsync(user, validRoles);
            }

            return Ok();
        }

        // GET api/permissions/claims — summary by role
        [HttpGet("claims")]
        public async Task<ActionResult<IEnumerable<ClaimGroupDto>>> GetRoleClaims()
        {
            var roles = await _roleManager.Roles.OrderBy(x => x.Name).ToListAsync();
            var rolePermissions = await _dbContext.RoleToPermissions.ToListAsync();
            var perms = await _dbContext.Permissions.Where(p => p.IsActived == true).ToDictionaryAsync(x => x.Id);

            var result = roles.Select(role => new ClaimGroupDto
            {
                RoleName = role.Name ?? string.Empty,
                Claims = rolePermissions
                    .Where(rp => rp.RoleId == role.Id && perms.ContainsKey(rp.PermissionId))
                    .Select(rp => perms[rp.PermissionId].Module + "." + perms[rp.PermissionId].Actions)
                    .Distinct()
                    .ToList()
            }).ToList();

            return Ok(result);
        }

        // POST api/permissions/seed
        [HttpPost("seed")]
        public async Task<IActionResult> Seed()
        {
            await PermissionSeeder.SeedAsync(_dbContext, _roleManager);
            return Ok(new { Status = "Success", Message = "Permissions seeded." });
        }
    }
}
