using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.RawAssemblingEntities;

namespace Assembler.Base.MessageEnrichers
{
    public class RawInitialFrameMessageEnricher : IMessageEnricher<BaseFrame, RawMessageInAssembly>
    {
        public void Enrich(BaseFrame frame, RawMessageInAssembly message)
        {
            message.InitialFrames.Add(frame);
        }
    }
}