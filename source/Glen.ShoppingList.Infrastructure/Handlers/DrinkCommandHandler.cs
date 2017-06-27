namespace Glen.ShoppingList.Infrastructure.Handlers
{
    using Api.Commands;
    using Commands;
    using EventSourcing;
    using Messaging.Handling;
    using Model.Commands;
    using WriteModel;

    public class DrinkCommandHandler : ICommandHandler<AddDrink>, ICommandHandler<UpdateDrinkQuantity>, ICommandHandler<DeleteDrink>
    {
        private readonly IEventSourcedRepository<Drink> _repository;

        public DrinkCommandHandler(IEventSourcedRepository<Drink> repository)
        {
            _repository = repository;
        }

        public void Handle(AddDrink command)
        {
            var drink = new Drink(command.Id, command.DrinkName, command.Quantity, command.CreatedBy);

            _repository.Save(drink, command.Id.ToString());
        }

        public void Handle(UpdateDrinkQuantity command)
        {
            var drink = _repository.Get(command.DrinkId);
            drink.UpdateDrinkQuantity(command.Quantity);
            _repository.Save(drink, command.Id.ToString());
        }

        public void Handle(DeleteDrink command)
        {
            var drink = _repository.Get(command.DrinkId);
            drink.Delete();
            _repository.Save(drink, command.Id.ToString());
        }
    }
}
