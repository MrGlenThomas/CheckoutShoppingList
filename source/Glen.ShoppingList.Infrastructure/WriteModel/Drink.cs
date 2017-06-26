namespace Glen.ShoppingList.Infrastructure.WriteModel
{
    using System;
    using System.Collections.Generic;
    using Events;
    using EventSourcing;

    public class Drink : EventSourced
    {
        public string Name { get; set; }

        public int Quantity { get; set; }

        protected Drink(Guid id)
            : base(id)
        {
            Handles<DrinkAdded>(OnDrinkAdded);
            Handles<DrinkQuantityUpdated>(OnQuantityUpdated);
            Handles<DrinkDeleted>(OnDrinkDeleted);
        }

        public Drink(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {
            LoadFrom(history);
        }

        public Drink(Guid id, string drinkName, int quantity, string createdBy)
            : this(id)
        {
            Update(new DrinkAdded { DrinkName = drinkName, Quantity = quantity, CreatedBy = createdBy });
        }

        public void UpdateDrinkQuantity(int quantity)
        {
            Update(new DrinkQuantityUpdated { Quantity = quantity });
        }

        public void Delete()
        {
            Update(new DrinkDeleted());
        }

        private void OnDrinkAdded(DrinkAdded @event)
        {
            Name = @event.DrinkName;
            Quantity = @event.Quantity;
        }

        private void OnQuantityUpdated(DrinkQuantityUpdated drinkQuantityUpdated)
        {
            Quantity = drinkQuantityUpdated.Quantity;
        }

        private void OnDrinkDeleted(DrinkDeleted drinkDeleted)
        {
            Quantity = 0;
        }
    }
}
