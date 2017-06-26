namespace Glen.ShoppingList.Infrastructure.Messaging
{
    using System.Collections.Generic;

    public class MessageSender : IMessageSender
    {
        /// <summary>
        /// Sends the specified message.
        /// </summary>
        public void Send(Message message)
        {
            InsertMessage(message);
        }

        /// <summary>
        /// Sends a batch of messages.
        /// </summary>
        public void Send(IEnumerable<Message> messages)
        {
            //TODO: transactional command sending...

            foreach (var message in messages)
            {
                InsertMessage(message);
            }
        }

        private void InsertMessage(Message message)
        {
            // Send message to receiver
        }
    }
}
