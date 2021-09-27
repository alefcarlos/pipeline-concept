using System;
using System.Collections.Generic;

namespace App
{
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
}