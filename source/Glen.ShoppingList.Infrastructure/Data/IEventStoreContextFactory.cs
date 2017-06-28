namespace Glen.ShoppingList.Infrastructure.Data
{
    public interface IEventStoreContextFactory
    {
        EventStoreContext GetInstance();
    }
}
