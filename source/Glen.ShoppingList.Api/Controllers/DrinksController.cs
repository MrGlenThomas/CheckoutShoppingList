namespace Glen.ShoppingList.Api.Controllers
{
    using System.Collections.Generic;
    using Commands;
    using Infrastructure;
    using Infrastructure.Messaging;
    using Infrastructure.ReadModel;
    using Microsoft.AspNetCore.Mvc;
    using Model;
    using ShoppingList.Model.Commands;

    [Route("api/[controller]")]
    public class DrinksController : Controller
    {
        private readonly IShoppingListDao _context;
        private readonly ICommandBus _bus;

        public DrinksController(IShoppingListDao context, ICommandBus bus)
        {
            _context = context;
            _bus = bus;
        }

        // GET api/drinks
        [HttpGet]
        public IEnumerable<ShoppingListDrink> Get()
        {
            return _context.AllDrinks();
        }

        // GET api/drinks/pepsi
        [HttpGet("{drinkName}")]
        public IActionResult Get(string drinkName)
        {
            var drinkId = _context.LocateDrink(drinkName);

            if (drinkId == null)
            {
                return NotFound();
            }

            var drink = _context.FindDrink(drinkId);

            return Ok(drink);
        }

        // POST api/drinks
        [HttpPost]
        public IActionResult Post([FromBody]AddDrinkRequest request)
        {
            var existingDrinkItem =
                _context.LocateDrink(request.DrinkName);

            if (existingDrinkItem != null)
            {
                // Drink already exists on shopping list.
                return StatusCode(422);
            }

            var command = new AddDrinks { DrinkName = request.DrinkName, Quantity = request.Quantity };
            _bus.Send(command);

            return Ok();
        }

        // PUT api/drinks/pepsi
        [HttpPut("{drinkName}")]
        public IActionResult Put(string drinkName, [FromBody]int quantity)
        {
            var existingDrinkId =
                _context.LocateDrink(drinkName);

            if (existingDrinkId == null)
            {
                // Drink doesn't exist on shopping list.
                return BadRequest();
            }

            var command = new UpdateDrinkQuantity { DrinkId = existingDrinkId.Value, DrinkName = drinkName, Quantity = quantity };
            _bus.Send(command);

            return Ok();
        }

        // DELETE api/drinks/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
