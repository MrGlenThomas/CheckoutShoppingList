namespace Glen.ShoppingList.Infrastructure
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using ReadModel;

    public class ShoppingListIdentityInitializer
    {
        private RoleManager<IdentityRole> _roleManager;
        private UserManager<ShoppingListUser> _userManager;

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
                
            }
        }
    }
}
