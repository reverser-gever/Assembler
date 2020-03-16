using System;
using System.ComponentModel;
using System.Net.WebSockets;
using Assembler.Core;
using Assembler.Core.Abstractions;
using Assembler.Core.Enums;

namespace Assembler.Base
{
    public class Assembler<T> : IAssembler
    {
        private readonly IResolver<MessageType, IHandler> _resolver;

        public event Action<BaseAssembledMessage> OnItemAssembled;

        public Assembler(IResolver<MessageType, IHandler> resolver,
            ITimeBasedCache<BaseAssembledMessage> cache)
        {
            _resolver = resolver;

            cache.OnItemExpired += ReleaseItem;
        }

        private void ReleaseItem(BaseAssembledMessage message)
        {
            message.ReleaseReason = ReleaseReason.TimeoutReached;
            OnItemAssembled?.Invoke(message);
        }

        public void Assemble(BaseFrame message)
        {
            var handler = _resolver.Resolve(message.MessageType);
            handler.Handle(message);
        }
    }
}