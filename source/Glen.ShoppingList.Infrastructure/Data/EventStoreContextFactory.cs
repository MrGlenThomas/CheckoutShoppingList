namespace Glen.ShoppingList.Infrastructure.Data
{
    using System;

    public class EventStoreContextFactory : IEventStoreContextFactory
    {
        private readonly Func<EventStoreContext> _factoryMethod;

        public EventStoreContextFactory(Func<EventStoreContext> factoryMethod)
        {
            _factoryMethod = factoryMethod;
        }

        public EventStoreContext GetInstance()
        {
            return _factoryMethod.Invoke();
        }
    }
}
