using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App
{
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
}