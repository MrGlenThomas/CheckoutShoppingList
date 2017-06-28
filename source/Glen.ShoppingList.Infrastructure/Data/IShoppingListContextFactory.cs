namespace Glen.ShoppingList.Infrastructure.Data
{
    public interface IShoppingListContextFactory
    {
        ShoppingListContext GetInstance();
    }
}
