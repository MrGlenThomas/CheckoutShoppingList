namespace Glen.ShoppingList.Infrastructure.ReadModel
{
    public class ShoppingListUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
