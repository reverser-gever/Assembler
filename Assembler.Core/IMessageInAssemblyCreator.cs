using Assembler.Core.Entities;

namespace Assembler.Core
{
    public interface IMessageInAssemblyCreator<out TMessage>
        where TMessage : BaseMessageInAssembly
    {
        TMessage Create();
    }
}