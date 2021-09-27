using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace App
{
    public static class ValidationPipelineBuilderExtensions
    {
        public static ValidationPipelineBuilder Apply(this ValidationPipelineBuilder builder, Func<ValidationContext, Func<Task>, Task> middleware)
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
        public static ValidationPipelineBuilder UseExceptionHandler(this ValidationPipelineBuilder builder)
        {
            return builder.Apply(async (context, next) =>
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
             });
        }

        //public static ValidationPipelineBuilder Apply(this ValidationPipelineBuilder builder, string validationName, Func<ValidationContext, Func<Task>, Task> middleware)
        //{
        //    return builder.Apply(next =>
        //    {
        //        return context =>
        //        {
        //            if (context.SkipedValidations.Contains(validationName))
        //            {
        //                Console.WriteLine("skiped " + validationName);
        //                return next(context);
        //            }
        //            else
        //            {
        //                Task simpleNext() => next(context);
        //                return middleware(context, simpleNext);
        //            }
        //        };
        //    });
        //}

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