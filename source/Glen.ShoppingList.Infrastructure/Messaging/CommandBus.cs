namespace Glen.ShoppingList.Infrastructure.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Serialization;

    /// <summary>
    /// Basic implementation of <see cref="ICommandBus"/>
    /// </summary>
    public class CommandBus : ICommandBus
    {
        private readonly IMessageSender _sender;
        private readonly ITextSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBus"/> class.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="serializer">The serializer to use for the message body.</param>
        public CommandBus(IMessageSender sender, ITextSerializer serializer)
        {
            _sender = sender;
            _serializer = serializer;
        }

        /// <summary>
        /// Sends the specified command.
        /// </summary>
        public void Send(Envelope<ICommand> command)
        {
            var message = BuildMessage(command);

            _sender.Send(message);
        }

        public void Send(IEnumerable<Envelope<ICommand>> commands)
        {
            var messages = commands.Select(BuildMessage);

            _sender.Send(messages);
        }

        private Message BuildMessage(Envelope<ICommand> command)
        {
            using (var payloadWriter = new StringWriter())
            {
                _serializer.Serialize(payloadWriter, command.Body);
                return new Message(payloadWriter.ToString(),
                    command.Delay != TimeSpan.Zero ? (DateTime?) DateTime.UtcNow.Add(command.Delay) : null,
                    command.CorrelationId);
            }
        }
    }
}
