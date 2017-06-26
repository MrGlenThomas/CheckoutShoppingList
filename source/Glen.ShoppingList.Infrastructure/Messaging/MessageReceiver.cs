namespace Glen.ShoppingList.Infrastructure.Messaging
{
    using System;

    public class MessageReceiver : IMessageReceiver
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
