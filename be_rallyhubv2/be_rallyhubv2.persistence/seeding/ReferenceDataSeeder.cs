using be_rallyhubv2.domain.entities;
using be_rallyhubv2.domain.enums;
using Microsoft.EntityFrameworkCore;

namespace be_rallyhubv2.persistence.seeding;

public sealed class ReferenceDataSeeder(AppDbContext db)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var roles = await db.Set<Role>().ToDictionaryAsync(role => role.RoleKey, cancellationToken);
        foreach (var (key, name) in ReferenceData.Roles)
        {
            if(roles.ContainsKey(key)) continue;

            var role = new Role
            {
                RoleKey = key,
                Name = name
            };

            db.Add(role);
            roles[key] = role;
        }
        
        var permissions = await db.Set<Permission>().ToDictionaryAsync(permission => permission.Code, cancellationToken);
        foreach (var (code, description) in ReferenceData.Permissions)
        {
            if(permissions.ContainsKey(code)) continue;

            var permission = new Permission
            {
                Code = code,
                Description = description
            };
            db.Add(permission);
            permissions[code] = permission;
        }
        
        var existingLinks = (await db.Set<RolePermission>()
                .Select(link => new {link.RoleId, link.PermissionId})
                .ToListAsync(cancellationToken))
            .Select(link => (link.RoleId, link.PermissionId))//đổi object thành tuple
            .ToHashSet();

        foreach (var (roleKey, codes) in ReferenceData.RolePermissions)
        {
            foreach (var code in codes)
            {
                var link = (RoleId: roles[roleKey].Id, PermissionId: permissions[code].Id);
                if (!existingLinks.Add(link)) continue;
                
                db.Add(new RolePermission {RoleId = link.RoleId, PermissionId = link.PermissionId});
            }
        }
        
        var existingKeys = await db.Set<Config>()
            .Where(x => x.Scope == ConfigScope.Platform)
            .Select(x => x.Key)
            .ToListAsync(cancellationToken);

        foreach (var (key, value) in ReferenceData.PlatformConfigs)
        {
            if(existingKeys.Contains(key)) continue;
            
            db.Add(new Config { Scope = ConfigScope.Platform, Key = key, Value = value });
        }
        
        await db.SaveChangesAsync(cancellationToken);
        
    }
    
    
}