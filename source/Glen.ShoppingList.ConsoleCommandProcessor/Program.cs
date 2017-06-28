namespace Glen.ShoppingList.ConsoleCommandProcessor
{
    using System;
    using Infrastructure;
    using Infrastructure.Data;
    using Infrastructure.EventSourcing;
    using Infrastructure.Handlers;
    using Infrastructure.Messaging;
    using Infrastructure.Messaging.Handling;
    using Infrastructure.Serialization;
    using Infrastructure.WriteModel;
    using Microsoft.EntityFrameworkCore;

    class Program
    {
        static void Main(string[] args)
        {
            Register();

            Console.ReadKey();
        }

        private static void Register()
        {
            var commandHandlerRegistry = new CommandProcessor(new DirectMessageReceiver(), new JsonTextSerializer());

            var drinksCommandHandler =
                new DrinkCommandHandler(
                    new EventSourcedRepository<Drink>(new EventBus(new PipesMessageSender(), new JsonTextSerializer()),
                        new JsonTextSerializer(),
                          new EventStoreContextFactory(() => new EventStoreContext(new DbContextOptionsBuilder<EventStoreContext>()
                            .UseInMemoryDatabase().Options))));

            commandHandlerRegistry.Register(drinksCommandHandler);
            
            commandHandlerRegistry.Start();
        }
    }
}