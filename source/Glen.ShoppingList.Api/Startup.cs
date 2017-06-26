namespace Glen.ShoppingList.Api
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Infrastructure;
    using Infrastructure.EventSourcing;
    using Infrastructure.Handlers;
    using Infrastructure.Messaging;
    using Infrastructure.Messaging.Handling;
    using Infrastructure.Serialization;
    using Infrastructure.WriteModel;
    using Swashbuckle.AspNetCore.Swagger;

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ShoppingListContext>(opt => opt.UseInMemoryDatabase());  // Is this required now using Dao?

            services.AddTransient<IShoppingListDao, ShoppingListDao>();
            services.AddTransient<ITextSerializer, JsonTextSerializer>();
            services.AddTransient<IMessageSender, DirectMessageSender>(); // Switch to PipesMessageSender
            //services.AddTransient<IMessageReceiver>(provider => commandMessageReceiver); // Switch to PipesMessageReceiver
            var commandMessageReceiver = new DirectMessageReceiver();
            var commandMessageSender = new DirectMessageSender(commandMessageReceiver);
            var eventMessageReceiver = new DirectMessageReceiver();
            var eventMessageSender = new DirectMessageSender(eventMessageReceiver);
            services.AddTransient<ICommandBus, CommandBus>(provider => new CommandBus(commandMessageSender, new JsonTextSerializer()));
            var eventBus = new EventBus(eventMessageSender, new JsonTextSerializer());
            var commandHandlerRegistry = new CommandProcessor(commandMessageReceiver, new JsonTextSerializer());

            var drinksCommandHandler =
                new DrinkCommandHandler(
                    new EventSourcedRepository<Drink>(eventBus,
                        new JsonTextSerializer(), () => new EventStoreContext(new DbContextOptionsBuilder<EventStoreContext>().UseInMemoryDatabase().Options)));

            commandHandlerRegistry.Register(drinksCommandHandler);

            commandHandlerRegistry.Start();

            services.AddTransient<ICommandHandlerRegistry>(provider => commandHandlerRegistry);

            var eventHandlerRegistry = new EventProcessor(eventMessageReceiver, new JsonTextSerializer());

            var drinkReadModelGenerator =
                new DrinkReadModelGenerator(() => new ShoppingListContext(new DbContextOptionsBuilder<ShoppingListContext>().UseInMemoryDatabase().Options));

            eventHandlerRegistry.Register(drinkReadModelGenerator);

            eventHandlerRegistry.Start();

            services.AddTransient<IEventHandlerRegistry>(provider => eventHandlerRegistry);

            services.AddTransient<IEventBus, EventBus>();

            services.AddTransient<Func<ShoppingListContext>>(s => () => new ShoppingListContext(new DbContextOptionsBuilder<ShoppingListContext>().UseInMemoryDatabase().Options));

            services.AddMvc();

            // Register the Swagger generator
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Checkout.com Shopping List", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseMvc();
        }
    }
}
