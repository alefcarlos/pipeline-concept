using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace App
{
    public interface IValidation
    {
        Task InvokeAsync(ValidationContext context, ValidationDelegate next);
    }

    public abstract class ValidationBase : IValidation
    {
        protected readonly ILogger logger;

        protected ValidationBase(ILogger logger)
        {
            this.logger = logger;
        }

        protected virtual bool ShouldApply(ValidationContext context)
        {
            return !context.SkipedValidations.Contains(GetType().Name);
        }

        public async Task InvokeAsync(ValidationContext context, ValidationDelegate next)
        {
            using (logger.BeginScope("Executando regra {ValidationName}", GetType().Name))
            {
                if (!ShouldApply(context))
                {
                    logger.LogWarning("Validation skiped");
                }
                else
                {
                    await ValidateAsync(context);
                }
            }

            await next.Invoke(context);
        }

        public abstract Task<bool> ValidateAsync(ValidationContext context);
    }
}