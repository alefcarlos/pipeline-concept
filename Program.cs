using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace App
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddScoped<IValidationFactory, ValidationFactory>();
            services.AddSingleton<CustomValidation>();

            using var provider = services.BuildServiceProvider();

            var builder = new ValidationPipelineBuilder();
            builder = builder.ApplyMastercardValidations();
            var pipe = builder.Build();


            var context = new ValidationContext(provider);

            context.SetMessage(new IsoMessage
            {
                Cvc = "123"
            });

            pipe.Invoke(context);
            Console.WriteLine(context.IsApproved);
            Console.WriteLine(context.Error.ToString());
        }
    }


    public static class MastercardValidationsExtensions
    {
        public static ValidationPipelineBuilder ApplyMastercardValidations(this ValidationPipelineBuilder builder)
        {
            return builder
            .Apply(async (context, next) =>
            {
                Console.WriteLine("Iniciando validação");
                await next();
                Console.WriteLine("Validação concluida");
            })
            .Apply(async (context, next) =>
            {
                Console.WriteLine("one");
                await next();
            })
            .Apply("validateMastercardCvc", async (context, next) =>
            {
                Console.WriteLine("validateMastercardCvc skip test");
                await next();
            })
            .Apply(async (context, next) =>
            {
                Console.WriteLine("two");

                await next();
            })
            .Apply(async (context, next) =>
            {
                Console.WriteLine("three");
                await next();
            }).Apply<CustomValidation>()
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


    public class CustomValidation : IValidation
    {
        public CustomValidation()
        {
            
        }

        public async Task InvokeAsync(ValidationContext context, ValidationDelegate next)
        {
            Console.WriteLine("Custom Validation logic from the separate class.");

            await next.Invoke(context);
        }
    }
}