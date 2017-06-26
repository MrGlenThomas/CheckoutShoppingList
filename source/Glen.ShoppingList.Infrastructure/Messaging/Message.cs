namespace Glen.ShoppingList.Infrastructure.Messaging
{
    using System;

    public class Message
    {
        public Message(string body, DateTime? deliveryDate = null, string correlationId = null)
        {
            Body = body;
            DeliveryDate = deliveryDate;
            CorrelationId = correlationId;
        }

        public string Body { get; }

        public string CorrelationId { get; }

        public DateTime? DeliveryDate { get; }
    }
}
