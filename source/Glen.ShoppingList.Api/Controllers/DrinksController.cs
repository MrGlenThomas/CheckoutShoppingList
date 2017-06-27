namespace Glen.ShoppingList.Api.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Commands;
    using Infrastructure;
    using Infrastructure.Commands;
    using Infrastructure.Messaging;
    using Infrastructure.ReadModel;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Model;
    using ShoppingList.Model.Commands;

    [Authorize]
    [Route("api/[controller]")]
    public class DrinksController : Controller
    {
        private readonly IShoppingListDao _context;
        private readonly ICommandBus _bus;
        private readonly ILogger<DrinksController> _logger;
        private readonly UserManager<ShoppingListUser> _userManager;

        public DrinksController(IShoppingListDao context, ICommandBus bus, ILoggerFactory logger,
            UserManager<ShoppingListUser> userManager)
        {
            _context = context;
            _bus = bus;
            _logger = logger.CreateLogger<DrinksController>();
            _userManager = userManager;
        }

        // GET api/drinks
        [HttpGet]
        public IEnumerable<ShoppingListDrink> Get()
        {
            _logger.LogInformation(LoggingEvents.GET_DRINKS, "Getting all drinks");

            return _context.AllDrinks();
        }

        // GET api/drinks/pepsi
        [HttpGet("{drinkName}")]
        public IActionResult Get(string drinkName)
        {
            _logger.LogInformation(LoggingEvents.GET_DRINK_BY_NAME, "Getting drink with name {0}", drinkName);

            var drinkId = _context.LocateDrink(drinkName);

            if (drinkId == null)
            {
                _logger.LogWarning(LoggingEvents.GET_DRINK_BY_NAME_NOT_FOUND, "Drink with name {0} not found", drinkName);

                return NotFound($"Drink not found with name '{drinkName}'");
            }

            var drink = _context.FindDrink(drinkId);

            return Ok(drink);
        }

        // POST api/drinks
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]AddDrinkRequest request)
        {
            _logger.LogInformation(LoggingEvents.POST_DRINK, "Adding drink with name {0} and quantity {1}", request.DrinkName, request.Quantity);

            var existingDrinkItem =
                _context.LocateDrink(request.DrinkName);

            if (existingDrinkItem != null)
            {
                _logger.LogWarning(LoggingEvents.POST_DRINK_ALREADY_EXISTS, "Adding drink with name {0} already exists", request.DrinkName);

                // Drink already exists on shopping list.
                return StatusCode(422, $"Drink '{request.DrinkName}' already exists");
            }

            var user = await _userManager.FindByNameAsync(this.User.Identity.Name);

            var command = new AddDrink { DrinkName = request.DrinkName, Quantity = request.Quantity, CreatedBy = user.UserName };
            _bus.Send(command);

            return Ok();
        }

        // PUT api/drinks/pepsi
        [HttpPut("{drinkName}")]
        public async Task<IActionResult> Put(string drinkName, [FromBody]int quantity)
        {
            _logger.LogInformation(LoggingEvents.PUT_DRINK, "Updating drink with name {0} and quantity {1}", drinkName, quantity);

            var existingDrinkId =
                _context.LocateDrink(drinkName);

            if (existingDrinkId == null)
            {
                _logger.LogInformation(LoggingEvents.PUT_DRINK_NOT_FOUND, "Updating drink with name {0} not found", drinkName, quantity);

                // Drink doesn't exist on shopping list.
                return NotFound($"Drink not found with name '{drinkName}'");
            }

            var drink = _context.FindDrink(existingDrinkId);

            var user = await _userManager.FindByNameAsync(this.User.Identity.Name);

            if (drink.CreatedBy != user.UserName) return Forbid();

            var command = new UpdateDrinkQuantity { DrinkId = existingDrinkId.Value, DrinkName = drinkName, Quantity = quantity };
            _bus.Send(command);

            return Ok();
        }

        // DELETE api/drinks/pepsi
        [HttpDelete("{drinkName}")]
        public async Task<IActionResult> Delete(string drinkName)
        {
            _logger.LogInformation(LoggingEvents.DELETE_DRINK, "Deleting drink with name", drinkName);

            var existingDrinkId =
                _context.LocateDrink(drinkName);

            if (existingDrinkId == null)
            {
                _logger.LogInformation(LoggingEvents.DELETE_DRINK_NOT_FOUND, "Deleting drink with name {0} not found", drinkName);

                // Drink doesn't exist on shopping list.
                return NotFound($"Drink not found with name '{drinkName}'");
            }

            var drink = _context.FindDrink(existingDrinkId);

            var user = await _userManager.FindByNameAsync(this.User.Identity.Name);

            if (drink.CreatedBy != user.UserName) return Forbid();

            var command = new DeleteDrink { DrinkId = existingDrinkId.Value };
            _bus.Send(command);

            return Ok();
        }
    }
}
