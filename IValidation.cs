using System.Threading.Tasks;

namespace App
{
    public interface IValidation
    {
        public Task InvokeAsync(TransactionContext context, ValidationDelegate next);
    }
}