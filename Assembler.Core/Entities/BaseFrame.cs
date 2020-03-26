using System;
using Assembler.Core.Enums;

namespace Assembler.Core.Entities
{
    public abstract class BaseFrame : BaseMessage
    {
        public AssemblingPosition AssemblingPosition { get; }

        protected BaseFrame(AssemblingPosition assemblingPosition)
        {
            AssemblingPosition = assemblingPosition;
        }

        protected BaseFrame(Guid guid, AssemblingPosition frameType) : base(guid)
        {
            AssemblingPosition = frameType;
        }
    }
}