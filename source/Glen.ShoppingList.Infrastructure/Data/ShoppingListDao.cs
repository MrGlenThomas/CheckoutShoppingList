namespace Glen.ShoppingList.Infrastructure.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ReadModel;

    public class ShoppingListDao : IShoppingListDao
    {
        private readonly ShoppingListContextFactory _contextFactory;

        public ShoppingListDao(ShoppingListContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public Guid? LocateDrink(string drinkName)
        {
            using (var context = _contextFactory.GetInstance())
            {
                var drinkProjection = context
                    .Query<ShoppingListDrink>()
                    .Where(o => o.DrinkName == drinkName)
                    .Select(o => new { o.Id })
                    .FirstOrDefault();

                return drinkProjection?.Id;
            }
        }

        public IEnumerable<ShoppingListDrink> AllDrinks(int pageSize, int pageNumber)
        {
            using (var context = _contextFactory.GetInstance())
            {
                return context.Drinks.Paginate(new PaginationArgs(pageSize, pageNumber)).ToArray();
            }
        }

        public ShoppingListDrink FindDrink(Guid? drinkId)
        {
            using (var context = _contextFactory.GetInstance())
            {
                return context.Query<ShoppingListDrink>().FirstOrDefault(dto => dto.Id == drinkId);
            }
        }
    }
}
