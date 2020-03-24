using Assembler.Core.Enums;

namespace Assembler.Core.Entities
{
    public abstract class BaseMessageInAssembly : BaseMessage
    {
        public ReleaseReason ReleaseReason { get; set; } = ReleaseReason.Unreleased;
        public bool MiddleReceived { get; set; } = false;
    }
}