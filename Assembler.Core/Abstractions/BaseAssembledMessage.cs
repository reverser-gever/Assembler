using System;
using Assembler.Core.Enums;

namespace Assembler.Core.Abstractions
{
    public abstract class BaseAssembledMessage : BaseMessage
    {
        public ReleaseReason ReleaseReason;

        protected BaseAssembledMessage()
        {
            ReleaseReason = ReleaseReason.Unreleased;
        }

        protected BaseAssembledMessage(Guid guid) : base(guid)
        {
            ReleaseReason = ReleaseReason.Unreleased;
        }
    }
}