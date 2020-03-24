using Assembler.Core.Enums;

namespace Assembler.Core.Entities
{
    public abstract class BaseMessageInAssembly : BaseMessage
    {
        public ReleaseReason ReleaseReason = ReleaseReason.Unreleased;
        public bool MiddleReceived = false;
    }
}