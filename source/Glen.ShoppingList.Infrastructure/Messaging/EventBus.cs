namespace Glen.ShoppingList.Infrastructure.Messaging
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Serialization;

    /// <summary>
    /// This is an extremely basic implementation of <see cref="IEventBus"/> that is used only for running the sample
    /// application without the dependency to the Windows Azure Service Bus when using the DebugLocal solution configuration.
    /// It should not be used in production systems.
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly IMessageSender _sender;
        private readonly ITextSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBus"/> class.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="serializer">The serializer to use for the message body.</param>
        public EventBus(IMessageSender sender, ITextSerializer serializer)
        {
            _sender = sender;
            _serializer = serializer;
        }

        /// <summary>
        /// Sends the specified event.
        /// </summary>
        public void Publish(Envelope<IEvent> @event)
        {
            var message = BuildMessage(@event);

            _sender.Send(message);
        }

        /// <summary>
        /// Publishes the specified events.
        /// </summary>
        public void Publish(IEnumerable<Envelope<IEvent>> events)
        {
            var messages = events.Select(BuildMessage);

            _sender.Send(messages);
        }

        private Message BuildMessage(Envelope<IEvent> @event)
        {
            using (var payloadWriter = new StringWriter())
            {
                _serializer.Serialize(payloadWriter, @event.Body);
                return new Message(payloadWriter.ToString(), correlationId: @event.CorrelationId);
            }
        }
    }
}
