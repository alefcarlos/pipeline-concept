using Microsoft.Extensions.DependencyInjection;
using System;

namespace App
{
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
}