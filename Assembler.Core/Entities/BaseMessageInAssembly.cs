using System;

namespace Assembler.Core.Entities
{
    public abstract class BaseMessageInAssembly : BaseMessage
    {
        public bool MiddleReceived { get; set; } = false;

        public DateTime AssemblingStartTime { get; set; } = DateTime.Now;

        public DateTime LastFrameReceived { get; set; } = DateTime.Now;
    }
}