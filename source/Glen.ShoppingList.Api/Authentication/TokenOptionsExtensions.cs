namespace Glen.ShoppingList.Api.Authentication
{
    using System;
    using System.Text;
    using Microsoft.IdentityModel.Tokens;

    public static class TokenOptionsExtensions
    {
        /// <summary>
        /// Returns a DateTime (UTC) indicating when the issued token should expire
        /// </summary>
        /// <param name="options"></param>
        /// <returns>When the issued token should expire</returns>
        public static DateTime GetExpiration(this TokenOptions options) => DateTime.UtcNow.Add(options.ValidFor);

        /// <summary>
        /// Returns an object that will be used for generating the token signature. HmacSha256 is the algorithm used.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static SigningCredentials GetSigningCredentials(this TokenOptions options) => new SigningCredentials(
            options.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256);

        /// <summary>
        /// Returns an object that wraps the value of the SigningKey as a byte array.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static SymmetricSecurityKey GetSymmetricSecurityKey(this TokenOptions options) =>
            new SymmetricSecurityKey(options.GetSigningKeyBytes());

        private static byte[] GetSigningKeyBytes(this TokenOptions options) => Encoding.ASCII.GetBytes(options
            .SigningKey);
    }
}
