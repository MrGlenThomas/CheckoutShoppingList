namespace Glen.ShoppingList.Infrastructure.ReadModel
{
    using System;

    public class ShoppingListDrink
    {
        public Guid Id { get; set; }

        public string DrinkName { get; set; }

        public int Quantity { get; set; }

        public string CreatedBy { get; set; }

        public ShoppingListDrink(Guid id, string drinkName, int quantity, string createdBy)
        {
            Id = id;
            DrinkName = drinkName;
            Quantity = quantity;
            CreatedBy = createdBy;
        }

        public ShoppingListDrink()
        {
            
        }
    }
}
