namespace Glen.ShoppingList.Api.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Infrastructure;
    using Infrastructure.ReadModel;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Model;

    public class AuthController : Controller
    {
        private readonly ShoppingListContext _context;
        private readonly SignInManager<ShoppingListUser> _signInManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ShoppingListContext context, SignInManager<ShoppingListUser> signInManager,
            ILogger<AuthController> logger)
        {
            _context = context;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost("api/auth/login")]
        public async Task<IActionResult> Login([FromBody] CredentialModel model)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);

                if (result.Succeeded)
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while logging in: {ex}");
            }

            return BadRequest("Failed to login");
        }
    }
}
