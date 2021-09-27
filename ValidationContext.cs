﻿using System;
using System.Collections.Generic;

namespace App
{
    public class ValidationContext
    {
        public IServiceProvider Services { get; }

        public EValidationError? Error { get; private set; }
        public bool IsApproved => !Error.HasValue;
        public IsoMessage Message { get; private set; }

        public ValidationContext(IServiceProvider services)
        {
            Services = services;
        }

        public void SetError(EValidationError error)
        {
            Error = error;
        }

        public void SetMessage(IsoMessage message)
        {
            Message = message;
        }

        public List<string> SkipedValidations { get; set; } = new List<string>
        {
            "validateMastercardCvc",
            "ValidationA"
        };

        public Dictionary<string, string> Parameters { get; private set; }

        public void SetParameters(Dictionary<string, string> parameters){
            Parameters = parameters;
        }

        public T ParameterValueAs<T>(string key)
        {
            var value = Parameters[key];

            if (typeof(T).IsEnum)
                return (T)Enum.Parse(typeof(T), value);

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}