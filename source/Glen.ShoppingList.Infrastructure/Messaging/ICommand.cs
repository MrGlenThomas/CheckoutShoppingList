namespace Glen.ShoppingList.Infrastructure.Messaging
{
    using System;

    public interface ICommand
    {
        /// <summary>
        /// Gets the command identifier.
        /// </summary>
        Guid Id { get; }
    }
}
