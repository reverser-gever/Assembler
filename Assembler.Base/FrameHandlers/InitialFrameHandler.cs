using System;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Assembler.Core.Releasing;
using Microsoft.Extensions.Logging;

namespace Assembler.Base.FrameHandlers
{
    public class InitialFrameHandler<TFrame, TMessageInAssembly> : BaseFrameHandler<TFrame, TMessageInAssembly>
        where TFrame : BaseFrame
        where TMessageInAssembly : BaseMessageInAssembly
    {
        private readonly TimeSpan _maxDifferenceBetweenTwoInitialFrames;

        public InitialFrameHandler(TimeSpan maxDifferenceBetweenInitialFrames,
            ITimeBasedCache<TMessageInAssembly> timeBasedCache, IIdentifierGenerator<TFrame> identifierGenerator,
            IMessageInAssemblyCreator<TMessageInAssembly> messageInAssemblyCreator,
            IMessageEnricher<TFrame, TMessageInAssembly> messageInAssemblyEnricher,
            IMessageInAssemblyReleaser<TMessageInAssembly> messageInAssemblyReleaser,
            IDateTimeProvider dateTimeProvider, ILoggerFactory loggerFactory)
            : base(timeBasedCache, identifierGenerator, messageInAssemblyEnricher, messageInAssemblyCreator,
                messageInAssemblyReleaser, dateTimeProvider, loggerFactory)
        {
            _maxDifferenceBetweenTwoInitialFrames = maxDifferenceBetweenInitialFrames;
        }

        public override void Handle(TFrame frame)
        {
            if (!TryGetIdentifier(frame, out var identifier)) return;

            TMessageInAssembly message = GetOrCreateMessageInAssembly(identifier, frame);

            EnrichMessage(frame, message);

            TimeBasedCache.Put(identifier, message);
        }

        private TMessageInAssembly GetOrCreateMessageInAssembly(string identifier, TFrame frame)
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
                message = ReleaseMessageAndCreateANewOne(identifier, message);
            }
            else
            {
                if (frame.StartTime - message.LastFrameReceived > _maxDifferenceBetweenTwoInitialFrames)
                {
                    message = ReleaseMessageAndCreateANewOne(identifier, message);
                }
            }

            return message;
        }

        private TMessageInAssembly ReleaseMessageAndCreateANewOne(string identifier, TMessageInAssembly message)
        {
            RemoveMessageFromCache(identifier, message);

            ReleaseMessage(message, ReleaseReason.AnotherMessageInitialized);

            return CreateMessage();
        }
    }
}