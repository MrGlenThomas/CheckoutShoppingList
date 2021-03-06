﻿namespace Glen.ShoppingList.Infrastructure.Data
{
    using System;
    using System.Collections.Generic;
    using ReadModel;

    public interface IShoppingListDao
    {
        Guid? LocateDrink(string drinkName);

        IEnumerable<ShoppingListDrink> AllDrinks(int pageSize, int page);

        ShoppingListDrink FindDrink(Guid? drinkId);
    }
}
