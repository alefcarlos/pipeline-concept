using System;
using System.Threading.Tasks;

namespace App
{
    public class ValidateInternationalTransaction : IValidation
    {
        public async Task InvokeAsync(ValidationContext context, ValidationDelegate next)
        {
            Console.WriteLine("Custom Validation logic from the separate class.");

            var input = new InternationalTransactionInput
            {
                OnlyDomestic = false,
            };

            Validation.With(context).Validate(input);

            await next.Invoke(context);
        }

        internal class Validation
        {
            private readonly ValidationContext context;

            public static Validation With(ValidationContext context)
            {
                return new Validation(context);
            }

            private Validation(ValidationContext context)
            {
                this.context = context;
            }

            public bool Validate(InternationalTransactionInput input)
            {
                var allowInternational = context.ParameterValueAs<bool>("AllowInternational");
                if (!allowInternational)
                {
                    context.SetError(EValidationError.InternationalTransactionNotAllowedForThisCountry);
                    return false;
                }

                if (input.OnlyDomestic)
                {
                    context.SetError(EValidationError.InternationalTransactionNotAllowedForDomesticCard);
                    return false;
                }

                return true;
            }
        }
    }
}