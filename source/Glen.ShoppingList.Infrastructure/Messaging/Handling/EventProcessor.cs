namespace Glen.ShoppingList.Infrastructure.Messaging.Handling
{
    using Messaging;
    using Serialization;

    /// <summary>
    /// Processes incoming events from the bus and routes them to the appropriate 
    /// handlers.
    /// </summary>
    public class EventProcessor : MessageProcessor, IEventHandlerRegistry
    {
        private readonly EventDispatcher _messageDispatcher;

        public EventProcessor(IMessageReceiver receiver, ITextSerializer serializer)
            : base(receiver, serializer)
        {
            _messageDispatcher = new EventDispatcher();
        }

        public void Register(IEventHandler eventHandler)
        {
            _messageDispatcher.Register(eventHandler);
        }

        protected override void ProcessMessage(object payload, string correlationId)
        {
            var @event = (IEvent)payload;
            _messageDispatcher.DispatchMessage(@event, null, correlationId, "");
        }
    }
}
