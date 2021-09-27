using Microsoft.Extensions.DependencyInjection;
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
            }).Apply<ValidateInternationalTransaction>()
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


    public class InternationalTransactionInput
    {
        public bool AllowInternational { get; set; }
        public bool OnlyDomestic { get; set; }
    }

    public class ValidateInternationalTransaction : IValidation
    {
        public async Task InvokeAsync(ValidationContext context, ValidationDelegate next)
        {
            Console.WriteLine("Custom Validation logic from the separate class.");

            var input = new InternationalTransactionInput
            {
                OnlyDomestic = false,
                AllowInternational = context.ParameterValueAs<bool>("AllowInternational")
            };

            Validation.New(context).Validate(input);

            await next.Invoke(context);
        }

        internal class Validation
        {
            private readonly ValidationContext context;

            public static Validation New(ValidationContext context)
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