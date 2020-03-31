using System;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Microsoft.Extensions.Logging;

namespace Assembler.Base.FrameHandlers
{
    public class InitialFrameHandler<TFrame, TMessageInAssembly> : BaseFrameHandler<TFrame, TMessageInAssembly>
        where TFrame : BaseFrame
        where TMessageInAssembly : BaseMessageInAssembly
    {
        public InitialFrameHandler(ITimeBasedCache<TMessageInAssembly> timeBasedCache,
            IIdentifierGenerator<TFrame> identifierGenerator,
            IMessageInAssemblyCreator<TMessageInAssembly> messageInAssemblyCreator,
            IMessageEnricher<TFrame, TMessageInAssembly> messageInAssemblyEnricher,
            IMessageReleaser<TMessageInAssembly> messageInAssemblyReleaser, IDateTimeProvider dateTimeProvider,
            ILoggerFactory loggerFactory)
            : base(timeBasedCache, identifierGenerator, messageInAssemblyEnricher, messageInAssemblyCreator,
                messageInAssemblyReleaser, dateTimeProvider, loggerFactory)
        { }

        public override void Handle(TFrame frame)
        {
            if (!TryGetIdentifier(frame, out var identifier)) return;

            TMessageInAssembly message = GetOrCreateMessageInAssembly(identifier);

            EnrichMessage(frame, message);

            TimeBasedCache.Put(identifier, message);
        }

        private TMessageInAssembly GetOrCreateMessageInAssembly(string identifier)
        {
            TMessageInAssembly message;

            if (!TimeBasedCache.Exists(identifier))
            {
                LogNoMessageInCache(identifier);
                message = CreateMessage();

                return message;
            }

            message = TimeBasedCache.Get(identifier);

            if (message.MiddleReceived)
            {
                RemoveMessageFromCache(identifier, message);

                ReleaseMessage(message, ReleaseReason.AnotherMessageInitialized);

                message = CreateMessage();
            }

            return message;
        }
    }
}