using System;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base
{
    public abstract class BaseFrameHandler<TFrame, TMessage> : IFrameHandler<TFrame, TMessage>
        where TFrame : BaseFrame
        where TMessage : BaseMessageInAssembly
    {
        protected readonly ITimeBasedCache<TMessage> TimeBasedCache;
        protected readonly IFactory<TFrame, string> IdentifierFactory;
        protected readonly IMessageEnricher<TFrame, TMessage> MessageInAssemblyEnricher;
        protected readonly ICreator<TMessage> MessageInAssemblyCreator;

        public event Action<TMessage, ReleaseReason> MessageAssemblyFinished;

        protected BaseFrameHandler(ITimeBasedCache<TMessage> timeBasedCache, IFactory<TFrame, string> identifierFactory,
            IMessageEnricher<TFrame, TMessage> messageInAssemblyEnricher, ICreator<TMessage> messageInAssemblyCreator)
        {
            TimeBasedCache = timeBasedCache;
            IdentifierFactory = identifierFactory;
            MessageInAssemblyEnricher = messageInAssemblyEnricher;
            MessageInAssemblyCreator = messageInAssemblyCreator;
        }

        public abstract void Handle(TFrame frame);

        protected void ReleaseMessage(TMessage message, ReleaseReason releaseReason)
        {
            MessageAssemblyFinished?.Invoke(message, releaseReason);
        }

        protected string GetIdentifier(TFrame frame) => IdentifierFactory.Create(frame);

        protected TMessage CreateMessage() => MessageInAssemblyCreator.Create();
    }
}