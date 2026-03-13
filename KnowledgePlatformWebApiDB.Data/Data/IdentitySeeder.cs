using KnowledgePlatformWebApiDB.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KnowledgePlatformWebApiDB.Data.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            // Apply migrations
            await context.Database.MigrateAsync();

            await SeedPermissions(context);
            await SeedRoles(roleManager);
            await SeedRolePermissions(context);
            await SeedUsers(userManager);
        }

        // --------------------------------------------------
        // PERMISSIONS
        // --------------------------------------------------
        private static async Task SeedPermissions(ApplicationDbContext context)
        {
            if (await context.Permissions.AnyAsync())
                return;

            var permissions = new List<Permission>
            {
                new Permission { Name = "AddProject" },
                new Permission { Name = "RemoveProject" },
                new Permission { Name = "AddTeam" },
                new Permission { Name = "RemoveTeam" },
                new Permission { Name = "AddMember" },
                new Permission { Name = "RemoveMember" },
                new Permission { Name = "ReadNote" },
                new Permission { Name = "WriteNote" }
            };

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
        }

        // --------------------------------------------------
        // ROLES
        // --------------------------------------------------
        private static async Task SeedRoles(RoleManager<ApplicationRole> roleManager)
        {
            string[] roles = { "ProjectAdmin", "ProjectLead", "TeamMember" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = role
                    });

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"Role Seeder Error: {error.Description}");
                        }
                    }
                }
            }
        }

        // --------------------------------------------------
        // ROLE PERMISSIONS
        // --------------------------------------------------
        private static async Task SeedRolePermissions(ApplicationDbContext context)
        {
            if (await context.RolePermissions.AnyAsync())
                return;

            var permissions = await context.Permissions.ToListAsync();

            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "ProjectAdmin");
            var leadRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "ProjectLead");
            var memberRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "TeamMember");

            if (adminRole == null || leadRole == null || memberRole == null)
                return;

            var rolePermissions = new List<RolePermission>();

            string[] adminPermissions =
            {
                "AddProject","RemoveProject","AddTeam","RemoveTeam",
                "AddMember","RemoveMember","ReadNote","WriteNote"
            };

            string[] leadPermissions =
            {
                "AddTeam","RemoveTeam",
                "AddMember","RemoveMember",
                "ReadNote","WriteNote"
            };

            string[] memberPermissions =
            {
                "ReadNote","WriteNote"
            };

            rolePermissions.AddRange(CreateRolePermissions(adminRole.Id, permissions, adminPermissions));
            rolePermissions.AddRange(CreateRolePermissions(leadRole.Id, permissions, leadPermissions));
            rolePermissions.AddRange(CreateRolePermissions(memberRole.Id, permissions, memberPermissions));

            await context.RolePermissions.AddRangeAsync(rolePermissions);
            await context.SaveChangesAsync();
        }

        private static IEnumerable<RolePermission> CreateRolePermissions(
            string roleId,
            List<Permission> permissions,
            string[] permissionNames)
        {
            return permissions
                .Where(p => permissionNames.Contains(p.Name))
                .Select(p => new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = p.Id
                });
        }

        // --------------------------------------------------
        // USERS
        // --------------------------------------------------
        private static async Task SeedUsers(UserManager<ApplicationUser> userManager)
        {
            var adminUser = await userManager.FindByNameAsync("admin");

            if (adminUser != null)
                return;

            var user = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@test.com",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Password@123");

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"User Seeder Error: {error.Description}");
                }
                return;
            }

            await userManager.AddToRoleAsync(user, "ProjectAdmin");
        }
    }
}