using System;
using Assembler.Core.Enums;

namespace Assembler.Core.Entities
{
    public abstract class BaseFrame
    {
        public Guid Guid { get; }
        public AssemblingPosition AssemblingPosition { get; }

        protected BaseFrame(Guid guid, AssemblingPosition frameType)
        {
            Guid = guid;
            AssemblingPosition = frameType;
        }

        protected BaseFrame(AssemblingPosition assemblingPosition) : this(Guid.NewGuid(), assemblingPosition)
        { }
    }
}