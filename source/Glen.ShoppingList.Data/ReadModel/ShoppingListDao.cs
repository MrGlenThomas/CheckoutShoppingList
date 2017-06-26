namespace Glen.ShoppingList.Data.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Model;

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
                    .Query<Drink>()
                    .Where(o => o.Name == drinkName)
                    .Select(o => new { o.Id })
                    .FirstOrDefault();

                if (drinkProjection != null)
                {
                    return drinkProjection.Id;
                }

                return null;
            }
        }

        public IEnumerable<Drink> AllDrinks()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Drinks.ToArray();
            }
        }

        public Drink FindDrink(Guid? drinkId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<Drink>().FirstOrDefault(dto => dto.Id == drinkId);
            }
        }
    }
}
