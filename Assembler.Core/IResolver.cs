namespace Assembler.Core
{
    public interface IResolver<in TIn, out TOut>
    {
        TOut Resolve(TIn input);
    }
}