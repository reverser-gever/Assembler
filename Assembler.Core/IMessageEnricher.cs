using Assembler.Core.Entities;

namespace Assembler.Core
{
    public interface IMessageEnricher<in TFrame, in TMessage>
        where TFrame : BaseFrame
        where TMessage : BaseMessageInAssembly
    {
        void Enrich(TFrame frame, TMessage message);
    }
}