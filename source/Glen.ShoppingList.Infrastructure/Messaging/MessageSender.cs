namespace Glen.ShoppingList.Infrastructure.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Pipes;
    using Newtonsoft.Json;

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

            var pipe = new NamedPipeServerStream("Glen.ShoppingList.Pipe", PipeDirection.InOut);
            pipe.WaitForConnection();

            var jsonMessage = JsonConvert.SerializeObject(message);

            var sw = new StreamWriter(pipe);

            sw.WriteLine(jsonMessage);
            sw.Flush();

            pipe.Disconnect();
        }
    }
}
