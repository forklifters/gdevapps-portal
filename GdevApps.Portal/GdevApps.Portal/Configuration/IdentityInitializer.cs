using GdevApps.Portal.Data;
using Microsoft.AspNetCore.Identity;

namespace GdevApps.Portal.Configuration
{
    internal static class IdentityInitializer
    {
        public static void SeedData(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            SeedRoles(roleManager);
            SeedUsers(userManager);
        }

        public static void SeedUsers(UserManager<ApplicationUser> userManager)
        {
            if (userManager.FindByEmailAsync("karac38@gmail.com").Result == null)
            {
                ApplicationUser user = new ApplicationUser();
                user.UserName = "Pasha";
                user.Email = "karac38@gmail.com";

                IdentityResult result = userManager.CreateAsync(user, "2?}2cZrsfBvg_?ap").Result;
                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user,"Admin").Wait();
                }
            }
        }

        public static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.RoleExistsAsync("Teacher").Result)
            {
                IdentityRole role = new IdentityRole();
                role.Name = "Teacher";
                IdentityResult roleResult = roleManager.
                CreateAsync(role).Result;
            }

            if (!roleManager.RoleExistsAsync("Admin").Result)
            {
                IdentityRole role = new IdentityRole();
                role.Name = "Admin";
                IdentityResult roleResult = roleManager.
                CreateAsync(role).Result;
            }

            if (!roleManager.RoleExistsAsync("Parent").Result)
            {
                IdentityRole role = new IdentityRole();
                role.Name = "Parent";
                IdentityResult roleResult = roleManager.
                CreateAsync(role).Result;
            }

            if (!roleManager.RoleExistsAsync("Student").Result)
            {
                IdentityRole role = new IdentityRole();
                role.Name = "Student";
                IdentityResult roleResult = roleManager.
                CreateAsync(role).Result;
            }
        }
    }
}