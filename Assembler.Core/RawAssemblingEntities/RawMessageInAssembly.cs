using System;
using System.Collections.Generic;
using Assembler.Core.Entities;

namespace Assembler.Core.RawAssemblingEntities
{
    public class RawMessageInAssembly : BaseMessageInAssembly
    {
        public List<BaseFrame> InitialFrames { get; }
        public List<BaseFrame> MiddleFrames { get; }
        public List<BaseFrame> FinalFrames { get; }

        public RawMessageInAssembly(DateTime assemblingStartTime, DateTime lastFrameReceived)
            : base(assemblingStartTime, lastFrameReceived)
        {
            InitialFrames = new List<BaseFrame>();
            MiddleFrames = new List<BaseFrame>();
            FinalFrames = new List<BaseFrame>();
        }
    }
}