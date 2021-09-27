using System.Threading.Tasks;

namespace App
{
    public interface IValidation
    {
        Task InvokeAsync(ValidationContext context, ValidationDelegate next);
    }
}