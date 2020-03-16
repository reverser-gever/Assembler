using System;

namespace Assembler.Core.Entities
{
    public abstract class BaseMessage
    {
        public Guid Guid { get; }

        protected BaseMessage(Guid guid)
        {
            Guid = guid;
        }

        protected BaseMessage()
        {
            Guid = Guid.NewGuid();
        }
    }
}