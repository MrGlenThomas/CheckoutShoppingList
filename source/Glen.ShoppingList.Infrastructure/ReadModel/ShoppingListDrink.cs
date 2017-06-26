namespace Glen.ShoppingList.Infrastructure.ReadModel
{
    using System;

    public class ShoppingListDrink
    {
        public Guid Id { get; set; }

        public string DrinkName { get; set; }

        public int Quantity { get; set; }

        public ShoppingListDrink(Guid id, string drinkName, int quantity)
        {
            Id = id;
            DrinkName = drinkName;
            Quantity = quantity;
        }

        public ShoppingListDrink()
        {
            
        }
    }
}
