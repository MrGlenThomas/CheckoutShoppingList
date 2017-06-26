namespace Glen.ShoppingList.Infrastructure.Messaging
{
    using System.Collections.Generic;

    public class DirectMessageSender : IMessageSender
    {
        private readonly IMessageReceiver _messageReceiver;

        public DirectMessageSender(IMessageReceiver messageReceiver)
        {
            _messageReceiver = messageReceiver;
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        public void Send(Message message)
        {
            _messageReceiver.ReceiveMessage(message);
        }

        public void Send(IEnumerable<Message> messages)
        {
            //TODO: transactional command sending...

            foreach (var message in messages)
            {
                _messageReceiver.ReceiveMessage(message);
            }
        }
    }
}
