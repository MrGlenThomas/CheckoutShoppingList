namespace Glen.ShoppingList.Infrastructure.Handlers
{
    using System;
    using Messaging.Handling;
    using Model.Events;
    using ReadModel;
    using WriteModel;

    public class DrinkReadModelGenerator : IEventHandler<DrinkAdded>
    {
        private readonly Func<ShoppingListContext> _contextFactory;

        public DrinkReadModelGenerator(Func<ShoppingListContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(DrinkAdded drinkAddedEvent)
        {
            using (var repository = _contextFactory.Invoke())
            {
                var dto = repository.Find<Drink>(drinkAddedEvent.SourceId);
                if (dto != null)
                {
                    // $"Ignoring DrinkAdded event for drink with ID {@event.SourceId} as it was already created."
                }
                else
                {
                    repository.Set<ShoppingListDrink>().Add(
                        new ShoppingListDrink(
                            drinkAddedEvent.SourceId,
                            drinkAddedEvent.DrinkName,
                            drinkAddedEvent.Quantity));

                    repository.SaveChanges();
                }
            }
        }
    }
}
