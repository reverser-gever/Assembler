using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base
{
    public abstract class BaseFrameHandler<TFrame, TMessageInAssembly> : IFrameHandler<TFrame>
        where TFrame : BaseFrame
        where TMessageInAssembly : BaseMessageInAssembly
    {
        protected readonly ITimeBasedCache<TMessageInAssembly> TimeBasedCache;
        protected readonly IFactory<TFrame, string> IdentifierFactory;
        protected readonly IMessageEnricher<TFrame, TMessageInAssembly> MessageInAssemblyEnricher;
        protected readonly ICreator<TMessageInAssembly> MessageInAssemblyCreator;
        protected readonly IMessageReleaser<TMessageInAssembly> MessageReleaser;

        protected BaseFrameHandler(ITimeBasedCache<TMessageInAssembly> timeBasedCache,
            IFactory<TFrame, string> identifierFactory,
            IMessageEnricher<TFrame, TMessageInAssembly> messageInAssemblyEnricher,
            ICreator<TMessageInAssembly> messageInAssemblyCreator, IMessageReleaser<TMessageInAssembly> messageReleaser)
        {
            TimeBasedCache = timeBasedCache;
            IdentifierFactory = identifierFactory;
            MessageInAssemblyEnricher = messageInAssemblyEnricher;
            MessageInAssemblyCreator = messageInAssemblyCreator;
            MessageReleaser = messageReleaser;
        }

        public abstract void Handle(TFrame frame);

        protected void ReleaseMessage(TMessageInAssembly message, ReleaseReason releaseReason) =>
            MessageReleaser.Release(message, releaseReason);

        protected string GetIdentifier(TFrame frame) => IdentifierFactory.Create(frame);

        protected TMessageInAssembly CreateMessage() => MessageInAssemblyCreator.Create();
    }
}