using Assembler.Core.Entities;

namespace Assembler.Core
{
    public interface IMessageEnricher<TFrame, TMessage>
    where TFrame : BaseFrame
    where TMessage : BaseMessageInAssembly
    {
        void Enrich(BaseFrame frame, BaseMessageInAssembly message);
    }
}