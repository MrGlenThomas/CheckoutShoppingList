namespace Glen.ShoppingList.Infrastructure.ReadModel
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

    public class ShoppingListUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
