namespace Glen.ShoppingList.Infrastructure.Data
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using ReadModel;

    public class ShoppingListIdentityInitializer
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ShoppingListUser> _userManager;

        public ShoppingListIdentityInitializer(RoleManager<IdentityRole> roleManager, UserManager<ShoppingListUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task Seed()
        {
            var user = await _userManager.FindByNameAsync("glenthomas");

            if (user == null)
            {
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    var role = new IdentityRole("Admin");
                    role.Claims.Add(new IdentityRoleClaim<string> { ClaimType = "IsAdmin", ClaimValue = "True" });
                    await _roleManager.CreateAsync(role);
                }

                user = new ShoppingListUser
                {
                    UserName = "glenthomas",
                    FirstName = "Glen",
                    LastName = "Thomas",
                    Email = "glen.thomas@outlook.com"
                };

                var userResult = await _userManager.CreateAsync(user, "P@ssw0rd!");
                var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
                var claimResult = await _userManager.AddClaimAsync(user, new Claim("SuperUser", "True"));

                if (!userResult.Succeeded || !roleResult.Succeeded || !claimResult.Succeeded)
                {
                    throw new InvalidOperationException("Failed to build user and roles");
                }
            }
        }
    }
}
