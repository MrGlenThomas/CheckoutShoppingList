namespace Glen.ShoppingList.Infrastructure.Messaging
{
    using System.Collections.Generic;

    public interface ICommandBus
    {
        void Send(Envelope<ICommand> command);

        void Send(IEnumerable<Envelope<ICommand>> commands);
    }
}
