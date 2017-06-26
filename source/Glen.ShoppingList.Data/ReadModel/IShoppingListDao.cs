namespace Glen.ShoppingList.Data.ReadModel
{
    using System;
    using System.Collections.Generic;
    using Model;

    public interface IShoppingListDao
    {
        Guid? LocateDrink(string drinkName);

        IEnumerable<Drink> AllDrinks();

        Drink FindDrink(Guid? drinkId);
    }
}
