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
            //Essa é o último middleware a ser executado,
            //se chegar aqui significa que a transação foi
            //autorizada
            ValidationDelegate validation = context =>
            {
                return new ValueTask();
            };

            for (int c = _components.Count - 1; c >= 0; c--)
            {
                validation = _components[c](validation);
            }

            return validation;
        }
    }
}