using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App
{
    internal class Program
    {
        static void Main(string[] args)
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
                {"AllowInternational", "false"}
            });

            context.SetMessage(new IsoMessage
            {
                Cvc = "123"
            });

            pipe.Invoke(context);
            Console.WriteLine(context.IsApproved);
            Console.WriteLine(context.Error.ToString());
        }
    }

    public class ValidationA : ValidationBase
    {
        public ValidationA(ILogger<ValidationA> logger) : base(logger)
        {
        }

        public override Task<bool> ValidateAsync(ValidationContext context)
        {
            return Task.FromResult(true);
        }
    }


    public class ValidationB : ValidationBase
    {
        public ValidationB(ILogger<ValidationB> logger) : base(logger)
        {
        }

        public override Task<bool> ValidateAsync(ValidationContext context)
        {
            throw new NotImplementedException();
            return Task.FromResult(true);
        }
    }


    public static class MastercardValidationsExtensions
    {
        public static ValidationPipelineBuilder ApplyMastercardValidations(this ValidationPipelineBuilder builder)
        {
            return builder
            .Apply(async (context, next) =>
            {
                var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("ValidationEngine");

                logger.LogInformation("Iniciando validação");

                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Uma expcetion não tratada ocorreu.");
                    context.SetError(EValidationError.UnhandledException);
                }
                finally
                {
                    logger.LogInformation("Validação concluida");
                }
            })
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