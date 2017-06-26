namespace Glen.ShoppingList.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Infrastructure.Messaging;

    public class AddDrink : ICommand, IValidatableObject
    {
        public Guid Id { get; }

        public string DrinkName { get; set; }

        public int Quantity { get; set; }

        public AddDrink()
        {
            Id = Guid.NewGuid();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Quantity <= 0)
            {
                return new[] { new ValidationResult("Quantity must be a positive integer.", new[] { "Quantity" }) };
            }

            return Enumerable.Empty<ValidationResult>();
        }
    }
}
