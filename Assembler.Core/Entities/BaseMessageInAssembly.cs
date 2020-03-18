using Assembler.Core.Enums;

namespace Assembler.Core.Entities
{
    public abstract class BaseMessageInAssembly : BaseMessage
    {
        public ReleaseReason ReleaseReason;
        public bool MiddleReceived;

        protected BaseMessageInAssembly()
        {
            ReleaseReason = ReleaseReason.Unreleased;
            MiddleReceived = false;
        }
    }
}