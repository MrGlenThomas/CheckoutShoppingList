namespace Glen.ShoppingList.Model
{
    using System;
    using System.Collections.Generic;
    using Events;
    using Infrastructure.EventSourcing;

    public class Drink : EventSourced
    {
        public string Name { get; set; }

        public int Quantity { get; set; }

        protected Drink(Guid id)
            : base(id)
        {
            Handles<DrinkQuantityUpdated>(OnQuantityUpdated);
        }

        public Drink(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {
            LoadFrom(history);
        }

        public Drink(Guid id, string drinkName, int quantity)
            : this(id)
        {
            Update(new DrinkAdded { DrinkName = drinkName, Quantity = quantity });
        }

        public void UpdateDrinkQuantity(int quantity)
        {
            Update(new DrinkQuantityUpdated { Quantity = quantity });
        }

        private void OnQuantityUpdated(DrinkQuantityUpdated drinkQuantityUpdated)
        {
            Quantity = drinkQuantityUpdated.Quantity;
        }
    }
}
