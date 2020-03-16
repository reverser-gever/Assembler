using Assembler.Core.Entities;

namespace Assembler.Core
{
    public interface IMessageEnricher<TFrame, TMessage>
    where TFrame : BaseFrame
    where TMessage : BaseMessage
    {
        void Enrich(BaseFrame frame, BaseMessage message);
    }
}