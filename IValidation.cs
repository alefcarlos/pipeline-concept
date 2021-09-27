using System.Threading.Tasks;

namespace App
{
    public interface IValidation
    {
        public Task InvokeAsync(ValidationContext context, ValidationDelegate next);
    }
}