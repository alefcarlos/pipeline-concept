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

            Console.WriteLine("Hello World!");

            var builder = new ValidationPipelineBuilder();
            builder = builder.ApplyMastercardValidations();
            var pipe = builder.Build();

            var context = new TransactionContext
            {
                Cvc = "123",
                Services = provider
            };

            pipe.Invoke(context);
            Console.WriteLine(context.IsApproved);

            context = new TransactionContext
            {
                Cvc = "321",
                Services = provider
            };

            pipe.Invoke(context);
            Console.WriteLine(context.IsApproved);
            Console.WriteLine(context.Error.ToString());
        }
    }


    public static class MastercardValidationsExtensions
    {
        public static ValidationPipelineBuilder ApplyMastercardValidations(this ValidationPipelineBuilder builder)
        {
            return builder.Apply(async (context, next) =>
            {
                Console.WriteLine("Iniciando validação");
                await next();
                Console.WriteLine("Validação concluida");
            }).Apply(async (context, next) =>
            {
                Console.WriteLine("one");
                await next();
            }).Apply(async (context, next) =>
            {
                Console.WriteLine("two");

                await next();
            }).Apply(async (context, next) =>
            {
                Console.WriteLine("three");
                await next();
            }).Apply<CustomValidation>()
            .ApplyCvc();
        }
    }


    public class CustomValidation : IValidation
    {
        public async Task InvokeAsync(TransactionContext context, ValidationDelegate next)
        {
            Console.WriteLine("Custom Validation logic from the separate class.");

            await next.Invoke(context);
        }
    }
}