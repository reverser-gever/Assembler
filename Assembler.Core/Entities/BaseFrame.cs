using System;
using Assembler.Core.Enums;

namespace Assembler.Core.Entities
{
    public abstract class BaseFrame : BaseMessage
    {
        public MessageType MessageType { get; }

        protected BaseFrame(MessageType messageType)
        {
            MessageType = messageType;
        }

        protected BaseFrame(Guid guid, MessageType messageType) : base(guid)
        {
            MessageType = messageType;
        }
    }
}