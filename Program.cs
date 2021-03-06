using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddScoped<IValidationFactory, ValidationFactory>();
            services.AddSingleton<ValidateInternationalTransaction>();
            services.AddSingleton<ValidationA>();
            services.AddSingleton<ValidationB>();
            services.AddLogging(opt => opt.AddConsole(configure => configure.IncludeScopes = true));

            using var provider = services.BuildServiceProvider();

            var builder = new ValidationPipelineBuilder();
            builder = builder.ApplyMastercardValidations();
            var pipe = builder.Build();

            var context = new ValidationContext(provider);

            context.SetParameters(new Dictionary<string, string> {
                { "AllowInternational", "false"},
                { "validateMastercardCvc_skip","true" },
                { "ValidationA_skip", "true" }
            });

            context.SetMessage(new IsoMessage
            {
                Cvc = "123"
            });

            await pipe.Invoke(context);

            Console.WriteLine(context.IsApproved);
            Console.WriteLine(context.Error.ToString());
        }
    }

    public class ValidationA : ValidationBase
    {
        public ValidationA(ILogger<ValidationA> logger) : base(logger)
        {
        }

        public override ValueTask<bool> ValidateAsync(ValidationContext context)
        {
            return new ValueTask<bool>(true);
        }
    }


    public class ValidationB : ValidationBase
    {
        public ValidationB(ILogger<ValidationB> logger) : base(logger)
        {
        }

        public override ValueTask<bool> ValidateAsync(ValidationContext context)
        {
            throw new NotImplementedException();
        }
    }


    public static class MastercardValidationsExtensions
    {
        public static ValidationPipelineBuilder ApplyMastercardValidations(this ValidationPipelineBuilder builder)
        {
            return builder
                .UseExceptionHandler()
                .Apply<ValidationA>()
                .Apply<ValidationB>()
                .Apply<ValidateInternationalTransaction>()
                .ApplyCvc();
        }

        public static ValidationPipelineBuilder ApplyCvc(this ValidationPipelineBuilder builder)
        {
            return builder.Apply(async (context, next) =>
            {
                if (context.Message.Cvc != "123")
                {
                    context.SetError(EValidationError.InvalidCvc);
                    return;
                }

                await next();
            });
        }
    }
}