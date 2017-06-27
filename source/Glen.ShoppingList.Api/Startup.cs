namespace Glen.ShoppingList.Api
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Infrastructure;
    using Infrastructure.Data;
    using Infrastructure.EventSourcing;
    using Infrastructure.Handlers;
    using Infrastructure.Messaging;
    using Infrastructure.Messaging.Handling;
    using Infrastructure.ReadModel;
    using Infrastructure.Serialization;
    using Infrastructure.WriteModel;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Swagger;

    public class Startup
    {
        private readonly IConfigurationRoot _configuration;
        private readonly IHostingEnvironment _environment;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            _environment = env;
            _configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_configuration);

            services.AddMemoryCache();

            services.AddDbContext<ShoppingListContext>(opt => opt.UseInMemoryDatabase());  // Is this required now using Dao?

            services.AddTransient<IShoppingListDao, ShoppingListDao>();
            services.AddTransient<ITextSerializer, JsonTextSerializer>();
            services.AddTransient<IMessageSender, DirectMessageSender>(); // Switch to PipesMessageSender
            //services.AddTransient<IMessageReceiver>(provider => commandMessageReceiver); // Switch to PipesMessageReceiver

            var commandMessageReceiver = new DirectMessageReceiver();
            var commandMessageSender = new DirectMessageSender(commandMessageReceiver);
            
            services.AddTransient<ICommandBus, CommandBus>(provider => new CommandBus(commandMessageSender, new JsonTextSerializer()));

            var eventMessageReceiver = new DirectMessageReceiver();
            var eventMessageSender = new DirectMessageSender(eventMessageReceiver);
            var eventBus = new EventBus(eventMessageSender, new JsonTextSerializer());

            var commandHandlerRegistry = new CommandProcessor(commandMessageReceiver, new JsonTextSerializer());
            var drinksCommandHandler =
                new DrinkCommandHandler(new EventSourcedRepository<Drink>(eventBus,
                        new JsonTextSerializer(), () => new EventStoreContext(new DbContextOptionsBuilder<EventStoreContext>().UseInMemoryDatabase().Options)));
            commandHandlerRegistry.Register(drinksCommandHandler);
            commandHandlerRegistry.Start();
            services.AddTransient<ICommandHandlerRegistry>(provider => commandHandlerRegistry);
            
            var drinkReadModelGenerator =
                new DrinkReadModelGenerator(() => new ShoppingListContext(new DbContextOptionsBuilder<ShoppingListContext>().UseInMemoryDatabase().Options));
            var eventHandlerRegistry = new EventProcessor(eventMessageReceiver, new JsonTextSerializer());
            eventHandlerRegistry.Register(drinkReadModelGenerator);
            eventHandlerRegistry.Start();
            services.AddTransient<IEventHandlerRegistry>(provider => eventHandlerRegistry);
            services.AddTransient<IEventBus, EventBus>();

            services.AddTransient<Func<ShoppingListContext>>(s => () => new ShoppingListContext(new DbContextOptionsBuilder<ShoppingListContext>().UseInMemoryDatabase().Options));

            services.AddTransient<ShoppingListIdentityInitializer>();
            services.AddIdentity<ShoppingListUser, IdentityRole>()
                .AddEntityFrameworkStores<ShoppingListContext>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Cookies.ApplicationCookie.Events = new CookieAuthenticationEvents()
                {
                    OnRedirectToLogin = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api")
                            && context.Response.StatusCode == 200)
                        {
                            context.Response.StatusCode = 401;
                        }

                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api")
                            && context.Response.StatusCode == 200)
                        {
                            context.Response.StatusCode = 403;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(cfg =>
            {
                cfg.AddPolicy("SuperUsers", p => p.RequireClaim("SuperUser", "True"));
            });

            services.AddMvc(options =>
            {
                if (!_environment.IsProduction())
                {
                    options.SslPort = 44345;
                }
                options.Filters.Add(new RequireHttpsAttribute());
            })
            .AddJsonOptions(opt =>
            {
                opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            // Register the Swagger generator
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Checkout.com Shopping List", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, ShoppingListIdentityInitializer shoppingListIdentityInitializer)
        {
            loggerFactory.AddConsole(_configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseIdentity();

            app.UseJwtBearerAuthentication(new JwtBearerOptions()
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = _configuration["Tokens:Issuer"],
                    ValidAudience = _configuration["Tokens:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"])),
                    ValidateLifetime = true
                }
            });

            app.UseMvc();

            shoppingListIdentityInitializer.Seed().Wait();
        }
    }
}
