namespace Glen.ShoppingList.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ReadModel;
    using WriteModel;

    public class ShoppingListDao : IShoppingListDao
    {
        private readonly Func<ShoppingListContext> _contextFactory;

        public ShoppingListDao(Func<ShoppingListContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public Guid? LocateDrink(string drinkName)
        {
            using (var context = _contextFactory.Invoke())
            {
                var drinkProjection = context
                    .Query<ShoppingListDrink>()
                    .Where(o => o.DrinkName == drinkName)
                    .Select(o => new { o.Id })
                    .FirstOrDefault();

                return drinkProjection?.Id;
            }
        }

        public IEnumerable<ShoppingListDrink> AllDrinks()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Drinks.ToArray();
            }
        }

        public ShoppingListDrink FindDrink(Guid? drinkId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<ShoppingListDrink>().FirstOrDefault(dto => dto.Id == drinkId);
            }
        }
    }
}
