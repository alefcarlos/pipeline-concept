namespace App
{
    public interface IValidationFactory
    {
        IValidation Create<T>() where T : IValidation;
    }
}