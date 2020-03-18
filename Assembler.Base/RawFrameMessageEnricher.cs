using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.RawAssemblingEntities;

namespace Assembler.Base
{
    public class RawFrameMessageEnricher : IMessageEnricher<BaseFrame, RawMessageInAssembly>
    {
        public void Enrich(BaseFrame frame, BaseMessageInAssembly message)
        {
            var rawMessage = message as RawMessageInAssembly;
            rawMessage?.AssembledFrames.Add(frame);
        }
    }
}