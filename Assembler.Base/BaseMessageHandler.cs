using System;
using Assembler.Core;
using Assembler.Core.Entities;

namespace Assembler.Base
{
    public abstract class BaseMessageHandler<TFrame, TMessage> : IHandler
        where TFrame : BaseFrame
        where TMessage : BaseMessageInAssembly
    {
        protected readonly ITimeBasedCache<TMessage> Cache;
        protected readonly IFactory<TFrame, string> IdentifierFactory;
        protected readonly IMessageEnricher<TFrame, TMessage> MessageEnricher;

        public event Action<BaseMessageInAssembly> MessageAssemblyFinished;

        protected BaseMessageHandler(ITimeBasedCache<TMessage> cache, IFactory<TFrame, string> identifierFactory,
            IMessageEnricher<TFrame, TMessage> messageEnricher)
        {
            Cache = cache;
            IdentifierFactory = identifierFactory;
            MessageEnricher = messageEnricher;
        }

        protected void ReleaseMessage(TMessage message)
        {
            MessageAssemblyFinished?.Invoke(message);
        }

        protected string GetIdentifier(TFrame frame) => IdentifierFactory.Create(frame);

        public abstract void Handle(BaseFrame message);
    }
}