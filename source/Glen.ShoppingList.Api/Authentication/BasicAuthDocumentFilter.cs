namespace Glen.ShoppingList.Api.Authentication
{
    using System.Collections.Generic;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class BasicAuthDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            var securityRequirements = new Dictionary<string, IEnumerable<string>>()
            {
                { "basic", new string[] { } }  // in swagger you specify empty list unless using OAuth2 scopes
            };

            swaggerDoc.Security = new[] { securityRequirements };
        }

    }
}
