using System.Collections.Generic;
using Assembler.Core.Entities;

namespace Assembler.Core.RawAssemblingEntities
{
    public class RawMessageInAssembly : BaseMessageInAssembly
    {
        public List<BaseFrame> AssembledFrames { get; }

        public RawMessageInAssembly()
        {
            AssembledFrames = new List<BaseFrame>();
        }
    }
}