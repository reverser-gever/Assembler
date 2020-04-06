namespace Assembler.Core
{
    public interface IResolver<in TIn, out TOut>
    {
        //TODO: Replace with the one we have
        TOut Resolve(TIn input);
    }
}