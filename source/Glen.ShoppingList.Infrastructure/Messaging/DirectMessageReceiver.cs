namespace Glen.ShoppingList.Infrastructure.Messaging
{
    using System;

    public class DirectMessageReceiver : IMessageReceiver
    {
        public string Name { get; set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void ReceiveMessage(Message message)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
        }
    }
}
