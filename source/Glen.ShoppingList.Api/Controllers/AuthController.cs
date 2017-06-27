namespace Glen.ShoppingList.Api.Controllers
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Authentication;
    using Filters;
    using Infrastructure.Data;
    using Infrastructure.ReadModel;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Model;
    using TokenOptions = Authentication.TokenOptions;

    public class AuthController : Controller
    {
        private readonly ShoppingListContext _context;
        private readonly SignInManager<ShoppingListUser> _signInManager;
        private readonly UserManager<ShoppingListUser> _userManager;
        private readonly IPasswordHasher<ShoppingListUser> _hasher;
        private readonly TokenOptions _options;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ShoppingListContext context, SignInManager<ShoppingListUser> signInManager,
            UserManager<ShoppingListUser> userManager, IOptions<TokenOptions> options,
            IPasswordHasher<ShoppingListUser> hasher,
            ILogger<AuthController> logger)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _hasher = hasher;
            _options = options.Value;
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

        [ValidateModel]
        [HttpPost("api/auth/token")]
        public async Task<IActionResult> CreateToken([FromBody] CredentialModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    if (_hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Success)
                    {
                        var userClaims = await _userManager.GetClaimsAsync(user);

                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                            new Claim(JwtRegisteredClaimNames.Email, user.Email)
                        }.Union(userClaims);

                        var credentials = _options.GetSigningCredentials();

                        var token = new JwtSecurityToken(
                            issuer: _options.Issuer,
                            audience: _options.Audience,
                            claims: claims,
                            expires: DateTime.UtcNow.AddMinutes(15),
                            signingCredentials: credentials
                        );

                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while creating JWT: {ex}");
            }

            return BadRequest("Failed to generate token");
        }
    }
}
