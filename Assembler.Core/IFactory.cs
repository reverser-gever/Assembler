namespace Assembler.Core
{
    public interface IFactory<in TIn, out TOut>
    {
        //TODO: Replace with the one we have
        TOut Create(TIn input);
    }
}