namespace Assembler.Core
{
    public interface IConverter<in TIn, out TOut>
    {
        //TODO: Replace with the one we have
        TOut Convert(TIn input);
    }
}