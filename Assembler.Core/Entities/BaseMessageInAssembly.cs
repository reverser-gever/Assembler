namespace Assembler.Core.Entities
{
    public abstract class BaseMessageInAssembly : BaseMessage
    {
        public bool MiddleReceived { get; set; } = false;
    }
}