namespace Glen.ShoppingList.Infrastructure.Data
{
    using System;

    public class ShoppingListContextFactory : IShoppingListContextFactory
    {
        private readonly Func<ShoppingListContext> _factoryMethod;

        public ShoppingListContextFactory(Func<ShoppingListContext> factoryMethod)
        {
            _factoryMethod = factoryMethod;
        }

        public ShoppingListContext GetInstance()
        {
            return _factoryMethod.Invoke();
        }
    }
}
