namespace Glen.ShoppingList.Infrastructure.Events
{
    using EventSourcing;

    public class DrinkQuantityUpdated : VersionedEvent
    {
        public int Quantity { get; set; }
    }
}
