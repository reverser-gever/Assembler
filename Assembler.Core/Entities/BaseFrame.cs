using System;
using Assembler.Core.Enums;

namespace Assembler.Core.Entities
{
    public abstract class BaseFrame : BaseMessage
    {
        public FrameType FrameType { get; }

        protected BaseFrame(FrameType frameType)
        {
            FrameType = frameType;
        }

        protected BaseFrame(Guid guid, FrameType frameType) : base(guid)
        {
            FrameType = frameType;
        }
    }
}