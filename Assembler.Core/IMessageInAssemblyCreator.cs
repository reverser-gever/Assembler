using Assembler.Core.Entities;

namespace Assembler.Core
{
    public interface IMessageInAssemblyCreator<T>
        where T : BaseMessageInAssembly
    {
        T Create();
    }
}