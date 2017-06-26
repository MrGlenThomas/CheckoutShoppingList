namespace Glen.ShoppingList.Infrastructure.Commands
{
    using System;
    using Messaging;

    public class DeleteDrink : ICommand
    {
        public Guid Id { get; }

        public Guid DrinkId { get; set; }

        public DeleteDrink()
        {
            Id = Guid.NewGuid();
        }
    }
}
