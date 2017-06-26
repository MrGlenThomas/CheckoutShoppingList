namespace Glen.ShoppingList.Infrastructure.Events
{
    using EventSourcing;

    public class DrinkAdded : VersionedEvent
    {
        public string DrinkName { get; set; }

        public int Quantity { get; set; }
    }
}
