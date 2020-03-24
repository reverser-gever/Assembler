using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.RawAssemblingEntities;

namespace Assembler.Base
{
    public class RawFrameMessageEnricher : IMessageEnricher<BaseFrame, RawMessageInAssembly>
    {
        public void Enrich(BaseFrame frame, RawMessageInAssembly message)
        {
            message.AssembledFrames.Add(frame);
        }
    }
}