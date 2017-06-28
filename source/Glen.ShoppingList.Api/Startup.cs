namespace Glen.ShoppingList.Api
{
    using System;
    using System.Threading.Tasks;
    using Authentication;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Infrastructure;
    using Infrastructure.Data;
    using Infrastructure.EventSourcing;
    using Infrastructure.Messaging;
    using Infrastructure.Messaging.Handling;
    using Infrastructure.ReadModel;
    using Infrastructure.Serialization;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;
    using StructureMap;
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
            
            services.AddIdentity<ShoppingListUser, IdentityRole>()
                .AddEntityFrameworkStores<ShoppingListContext>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Cookies.ApplicationCookie.Events = new CookieAuthenticationEvents
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

            services.AddOptions();
            services.Configure<TokenOptions>(_configuration.GetSection(nameof(TokenOptions)));

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
                //c.AddSecurityDefinition("basic", new BasicAuthScheme { Type = "basic" });
                //c.DocumentFilter<BasicAuthDocumentFilter>();
            });
        }

        public void ConfigureContainer(Registry registry)
        {
            // Use StructureMap-specific APIs to register services in the registry.

            registry.Scan(_ =>
            {
                _.AssemblyContainingType(typeof(Startup));
                _.Assembly("Glen.ShoppingList.Infrastructure");
                _.WithDefaultConventions();
                _.AddAllTypesOf<ICommandHandler>();
                _.AddAllTypesOf<IEventHandler>();
            });

            registry.For(typeof(IEventSourcedRepository<>)).Use(typeof(EventSourcedRepository<>));
            registry.For<ITextSerializer>().Use<JsonTextSerializer>();
            registry.For<IMessageReceiver>().Use<DirectMessageReceiver>().Singleton().Named("CommandMessageReceiver").SetProperty(receiver => receiver.Name = "CommandMessageReceiver");
            registry.For<IMessageReceiver>().Use<DirectMessageReceiver>().Singleton().Named("EventMessageReceiver").SetProperty(receiver => receiver.Name = "EventMessageReceiver"); ;
            registry.For<IMessageSender>().Add<DirectMessageSender>().Named("CommandMessageSender").Ctor<IMessageReceiver>().IsNamedInstance("CommandMessageReceiver");
            registry.For<IMessageSender>().Use<DirectMessageSender>().Named("EventMessageSender").Ctor<IMessageReceiver>().IsNamedInstance("EventMessageReceiver");
            registry.For<ICommandBus>().Use<CommandBus>().Ctor<IMessageSender>().IsNamedInstance("CommandMessageSender"); // CommandBus needs to use commandMessageSender instance
            registry.For<IEventBus>().Use<EventBus>().Ctor<IMessageSender>().IsNamedInstance("EventMessageSender"); // EventBus needs to use eventMessageSender instance
            registry.For<ICommandHandlerRegistry>().Use<CommandProcessor>().Singleton().Ctor<IMessageReceiver>().IsNamedInstance("CommandMessageReceiver");
            registry.For<IEventHandlerRegistry>().Use<EventProcessor>().Singleton().Ctor<IMessageReceiver>().IsNamedInstance("EventMessageReceiver");
            registry.Forward<ICommandHandlerRegistry, IProcessor>();
            registry.Forward<IEventHandlerRegistry, IProcessor>();
            registry.For<ShoppingListContext>().Use(_ => new ShoppingListContext(new DbContextOptionsBuilder<ShoppingListContext>().UseInMemoryDatabase(null).Options)).AlwaysUnique();
            registry.For<IShoppingListContextFactory>().Use<ShoppingListContextFactory>().Ctor<Func<ShoppingListContext>>().Is(() => new ShoppingListContext(new DbContextOptionsBuilder<ShoppingListContext>().UseInMemoryDatabase().Options)).AlwaysUnique();
            registry.For<IEventStoreContextFactory>().Use<EventStoreContextFactory>().Ctor<Func<EventStoreContext>>().Is(() => new EventStoreContext(new DbContextOptionsBuilder<EventStoreContext>().UseInMemoryDatabase().Options)).AlwaysUnique();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, ShoppingListIdentityInitializer shoppingListIdentityInitializer, IContainer container)
        {
            loggerFactory.AddConsole(_configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Register command handlers with command handler registry
            var commandHandlerRegistry = app.ApplicationServices.GetService<ICommandHandlerRegistry>();
            foreach (var commandHandler in app.ApplicationServices.GetServices<ICommandHandler>())
            {
                commandHandlerRegistry.Register(commandHandler);
            }

            // Register event handlers with event handler registry
            var eventHandlerRegistry = app.ApplicationServices.GetService<IEventHandlerRegistry>();
            foreach (var eventHandler in app.ApplicationServices.GetServices<IEventHandler>())
            {
                eventHandlerRegistry.Register(eventHandler);
            }

            // Start the command/event processors. In a real-world scenario these would be running in an external process
            foreach (var processor in app.ApplicationServices.GetServices<IProcessor>())
            {
                processor.Start();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("https://localhost:44345/swagger/v1/swagger.json", "Shopping List API V1");
            });

            app.UseIdentity();

            var options = _configuration.GetSection(nameof(TokenOptions)).Get<TokenOptions>();

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = options.Issuer,
                    ValidAudience = options.Audience,
                    IssuerSigningKey = options.GetSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true
                }
            });

            app.UseMvc();

            shoppingListIdentityInitializer.Seed().Wait();
        }
    }
}
