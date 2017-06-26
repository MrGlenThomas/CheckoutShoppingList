namespace Glen.ShoppingList.Infrastructure
{
    using System;
    using Microsoft.EntityFrameworkCore;

    public class EventStoreContext : DbContext
    {
    }

    public class Event
    {
        public Guid AggregateId { get; set; }
        public string AggregateType { get; set; }
        public int Version { get; set; }
        public string Payload { get; set; }
        public string CorrelationId { get; set; }

        // TODO: Following could be very useful for when rebuilding the read model from the event store, 
        // to avoid replaying every possible event in the system
        // public string EventType { get; set; }
    }
}
