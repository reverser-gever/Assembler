namespace Assembler.Core
{
    public interface ICreator<out TOut>
    {
        TOut Create();
    }
}