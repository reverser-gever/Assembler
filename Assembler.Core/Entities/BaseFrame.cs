using System;
using Assembler.Core.Enums;

namespace Assembler.Core.Entities
{
    public abstract class BaseFrame
    {
        public Guid Guid { get; }
        public AssemblingPosition AssemblingPosition { get; }
        public DateTime StartTime { get; }

        protected BaseFrame(Guid guid, AssemblingPosition frameType, DateTime startTime)
        {
            Guid = guid;
            AssemblingPosition = frameType;
            StartTime = startTime;
        }

        protected BaseFrame(AssemblingPosition assemblingPosition, DateTime startTime) : this(Guid.NewGuid(),
            assemblingPosition, startTime)
        {
        }
    }
}