using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace App
{
    public class ValidateInternationalTransaction : ValidationBase
    {
        public ValidateInternationalTransaction(ILogger<ValidateInternationalTransaction> logger) : base(logger)
        {
        }

        public override Task<bool> ValidateAsync(ValidationContext context)
        {
            var input = new InternationalTransactionInput
            {
                OnlyDomestic = false,
                AllowInternational = context.ParameterValueAs<bool>("AllowInternational")

            };

            logger.LogInformation("teste");

            return Task.FromResult(Validation.With(context).Validate(input));
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
                if (!input.AllowInternational)
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