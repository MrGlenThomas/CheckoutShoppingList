namespace Glen.ShoppingList.Model.Events
{
    using Infrastructure.EventSourcing;

    public class DrinkQuantityUpdated : VersionedEvent
    {
        public int Quantity { get; set; }
    }
}
