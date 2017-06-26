namespace Glen.ShoppingList.Infrastructure
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class EntityNotFoundException : Exception
    {
        private readonly Guid entityId;
        private readonly string entityType;

        public EntityNotFoundException()
        {
        }

        public EntityNotFoundException(Guid entityId) : base(entityId.ToString())
        {
            this.entityId = entityId;
        }

        public EntityNotFoundException(Guid entityId, string entityType)
            : base(entityType + ": " + entityId.ToString())
        {
            this.entityId = entityId;
            this.entityType = entityType;
        }

        public EntityNotFoundException(Guid entityId, string entityType, string message, Exception inner)
            : base(message, inner)
        {
            this.entityId = entityId;
            this.entityType = entityType;
        }

        protected EntityNotFoundException(
            SerializationInfo info,
            StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            entityId = Guid.Parse(info.GetString("entityId"));
            entityType = info.GetString("entityType");
        }

        public Guid EntityId => entityId;

        public string EntityType => entityType;
    }
}
