namespace Glen.ShoppingList.Infrastructure.Handlers
{
    using Data;
    using Events;
    using Messaging.Handling;
    using ReadModel;

    public class DrinkReadModelGenerator : IEventHandler<DrinkAdded>, IEventHandler<DrinkQuantityUpdated>, IEventHandler<DrinkDeleted>
    {
        private readonly ShoppingListContextFactory _contextFactory;

        public DrinkReadModelGenerator(ShoppingListContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(DrinkAdded drinkAddedEvent)
        {
            using (var repository = _contextFactory.GetInstance())
            {
                var dto = repository.Find<ShoppingListDrink>(drinkAddedEvent.SourceId);
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
                            drinkAddedEvent.Quantity,
                            drinkAddedEvent.CreatedBy));

                    repository.SaveChanges();
                }
            }
        }

        public void Handle(DrinkQuantityUpdated drinkQuantityUpdatedEvent)
        {
            using (var repository = _contextFactory.GetInstance())
            {
                var dto = repository.Find<ShoppingListDrink>(drinkQuantityUpdatedEvent.SourceId);
                if (dto == null)
                {
                    throw new EntityNotFoundException(drinkQuantityUpdatedEvent.SourceId, typeof(ShoppingListDrink).Name);
                }

                dto.Quantity = drinkQuantityUpdatedEvent.Quantity;

                repository.Set<ShoppingListDrink>().Update(dto);

                repository.SaveChanges();
            }
        }

        public void Handle(DrinkDeleted drinkDeletedEvent)
        {
            using (var repository = _contextFactory.GetInstance())
            {
                var dto = repository.Find<ShoppingListDrink>(drinkDeletedEvent.SourceId);
                if (dto == null)
                {
                    throw new EntityNotFoundException(drinkDeletedEvent.SourceId, typeof(ShoppingListDrink).Name);
                }

                repository.Set<ShoppingListDrink>().Remove(dto);

                repository.SaveChanges();
            }
        }
    }
}
