namespace Glen.ShoppingList.Model.Events
{
    using Infrastructure.EventSourcing;

    public class DrinkAdded : VersionedEvent
    {
        public string DrinkName { get; set; }

        public int Quantity { get; set; }
    }
}
