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

    public interface IValidationFactory
    {
        IValidation Create<T>() where T : IValidation;
    }

    public class ValidationFactory : IValidationFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IValidation Create<T>() where T : IValidation
        {
            return _serviceProvider.GetRequiredService<T>();
        }
    }

    public interface IValidation
    {
        public Task InvokeAsync(TransactionContext context, ValidationDelegate next);
    }

    public class ValidationOne
    {
        public Task Execute(TransactionContext context)
        {
            return Task.CompletedTask;
        }
    }

    public delegate Task ValidationDelegate(TransactionContext context);

    public class TransactionContext
    {
        public IServiceProvider Services { get; set; }
        public EValidationError? Error { get; set; }
        public bool IsApproved => !Error.HasValue;
        public string Cvc { get; set; }

        public List<string> SkipedValidations { get; set; } = new List<string>
        {
            "validateMastercardCvc"
        };
    }

    public class ValidationPipelineBuilder
    {
        private readonly List<Func<ValidationDelegate, ValidationDelegate>> _components = new();

        public ValidationPipelineBuilder Apply(Func<ValidationDelegate, ValidationDelegate> middleware)
        {
            _components.Add(middleware);
            return this;
        }

        public ValidationDelegate Build()
        {
            ValidationDelegate validation = context =>
            {
                //Significa que a transação foi autorizada
                Console.WriteLine("Approved");
                return Task.CompletedTask;
            };

            for (int c = _components.Count - 1; c >= 0; c--)
            {
                validation = _components[c](validation);
            }

            return validation;
        }
    }

    public enum EValidationError
    {
        InvalidCvc
    }

    public class CustomValidation : IValidation
    {
        public async Task InvokeAsync(TransactionContext context, ValidationDelegate next)
        {
            Console.WriteLine("Custom Validation logic from the separate class.");

            await next.Invoke(context);
        }
    }

    public static class Validations
    {
        public static ValidationPipelineBuilder ApplyCvc(this ValidationPipelineBuilder builder)
        {
            return builder.Apply(async (context, next) =>
            {
                if (context.Cvc != "123")
                {
                    context.Error = EValidationError.InvalidCvc;
                    return;
                }

                await next();
            });
        }

        public static ValidationPipelineBuilder Apply(this ValidationPipelineBuilder builder, Func<TransactionContext, Func<Task>, Task> middleware)
        {
            return builder.Apply(next =>
            {
                return context =>
                {
                    Task simpleNext() => next(context);
                    return middleware(context, simpleNext);
                };
            });
        }

        public static ValidationPipelineBuilder Apply<T>(this ValidationPipelineBuilder builder)
            where T : IValidation
        {
            return builder.Apply(next =>
            {
                return async context =>
                {
                    var factory = context.Services.GetRequiredService<IValidationFactory>();

                    var validation = factory.Create<T>();

                    await validation.InvokeAsync(context, next);
                };
            });
        }
    }
}