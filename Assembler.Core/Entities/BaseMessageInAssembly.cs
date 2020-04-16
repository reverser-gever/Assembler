using System;
using System.Collections.Generic;
using Assembler.Core.Enums;

namespace Assembler.Core.Entities
{
    public abstract class BaseMessageInAssembly
    {
        public Guid Guid { get; }
        public ReleaseReason ReleaseReason { get; set; }
        public bool MiddleReceived { get; set; }
        public DateTime AssemblingStartTime { get; set; }
        public DateTime LastFrameReceived { get; set; }
        public List<Guid> BasedOnFramesGuids { get; }

        protected BaseMessageInAssembly(DateTime assemblingStartTime, DateTime lastFrameReceived,
            bool middleReceived = false)
        {
            Guid = Guid.NewGuid();
            ReleaseReason = ReleaseReason.Unreleased;
            MiddleReceived = middleReceived;
            AssemblingStartTime = assemblingStartTime;
            LastFrameReceived = lastFrameReceived;
            BasedOnFramesGuids = new List<Guid>();
        }
    }
}