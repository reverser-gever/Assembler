namespace Assembler.Core
{
    public interface IValidator<in T>
    {
        bool IsValid(T input);
    }
}