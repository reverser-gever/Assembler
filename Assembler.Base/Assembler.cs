using System;
using System.Collections.Generic;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base
{
    public class Assembler<T> : IAssembler
    {
        private readonly IResolver<MessageType, IHandler> _resolver;

        public event Action<BaseMessageInAssembly> OnItemAssembled;

        public Assembler(IResolver<MessageType, IHandler> resolver,
            ITimeBasedCache<BaseMessageInAssembly> cache,
            IEnumerable<IAssemblyFinishHandler> handlersWithEvents)
        {
            _resolver = resolver;

            cache.OnItemExpired += ReleaseExpiredMessage;

            foreach (var assemblyFinishHandler in handlersWithEvents)
            {
                assemblyFinishHandler.OnMessageAssembled += ReleaseMessage;
            }
        }

        public void Assemble(BaseFrame message)
        {
            var handler = _resolver.Resolve(message.MessageType);
            handler.Handle(message);
        }

        private void ReleaseExpiredMessage(BaseMessageInAssembly message)
        {
            message.ReleaseReason = ReleaseReason.TimeoutReached;
            ReleaseMessage(message);
        }

        private void ReleaseMessage(BaseMessageInAssembly message)
        {
            OnItemAssembled?.Invoke(message);
        }
    }
}