using System;

namespace Assembler.Core.Abstractions
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