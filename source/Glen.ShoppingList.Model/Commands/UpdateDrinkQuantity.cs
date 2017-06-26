namespace Glen.ShoppingList.Api.Commands
{
    using System;
    using Infrastructure.Messaging;

    public class UpdateDrinkQuantity : ICommand
    {
        public Guid Id { get; }

        public string DrinkName { get; set; }

        public int Quantity { get; set; }

        public Guid DrinkId { get; set; }

        public UpdateDrinkQuantity()
        {
            Id = Guid.NewGuid();
        }
    }
}
