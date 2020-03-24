using System;
using Assembler.Core;
using Assembler.Core.Entities;

namespace Assembler.Base
{
    public abstract class BaseFrameHandler<TFrame, TMessage> : IFrameHandler<TFrame, TMessage>
        where TFrame : BaseFrame
        where TMessage : BaseMessageInAssembly
    {
        protected readonly ITimeBasedCache<TMessage> Cache;
        protected readonly IFactory<TFrame, string> IdentifierFactory;
        protected readonly IMessageEnricher<TFrame, TMessage> MessageEnricher;
        protected readonly ICreator<TMessage> MessageInAssemblyCreator;

        public event Action<TMessage> MessageAssemblyFinished;

        public abstract void Handle(TFrame frame);

        protected BaseFrameHandler(ITimeBasedCache<TMessage> cache, IFactory<TFrame, string> identifierFactory,
            IMessageEnricher<TFrame, TMessage> messageEnricher, ICreator<TMessage> messageInAssemblyCreator)
        {
            Cache = cache;
            IdentifierFactory = identifierFactory;
            MessageEnricher = messageEnricher;
            MessageInAssemblyCreator = messageInAssemblyCreator;
        }

        protected void ReleaseMessage(TMessage message)
        {
            MessageAssemblyFinished?.Invoke(message);
        }

        protected string GetIdentifier(TFrame frame) => IdentifierFactory.Create(frame);

        protected TMessage CreateMessage() => MessageInAssemblyCreator.Create();
    }
}