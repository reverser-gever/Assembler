using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Assembler.Core.Releasing;
using Microsoft.Extensions.Logging;

namespace Assembler.Base.FrameHandlers
{
    public class FinalFrameHandler<TFrame, TMessageInAssembly> : BaseFrameHandler<TFrame, TMessageInAssembly>
        where TFrame : BaseFrame
        where TMessageInAssembly : BaseMessageInAssembly
    {
        public FinalFrameHandler(ITimeBasedCache<TMessageInAssembly> timeBasedCache,
            IIdentifierGenerator<TFrame> identifierGenerator,
            IMessageInAssemblyCreator<TMessageInAssembly> messageInAssemblyCreator,
            IMessageEnricher<TFrame, TMessageInAssembly> messageInAssemblyEnricher,
            IMessageInAssemblyReleaser<TMessageInAssembly> messageInAssemblyReleaser, IDateTimeProvider dateTimeProvider,
            ILoggerFactory loggerFactory)
            : base(timeBasedCache, identifierGenerator, messageInAssemblyEnricher, messageInAssemblyCreator,
                messageInAssemblyReleaser, dateTimeProvider, loggerFactory)
        {
        }

        public override void Handle(TFrame frame)
        {
            if (!TryGetIdentifier(frame, out var identifier)) return;

            var message = GetOrCreateMessageInAssembly(identifier);

            EnrichMessage(frame, message);

            ReleaseMessage(message, ReleaseReason.FinalFrameReceived);
        }

        private TMessageInAssembly GetOrCreateMessageInAssembly(string identifier)
        {
            if (!TimeBasedCache.Exists(identifier))
            {
                LogNoMessageInCache(identifier);
                return CreateMessage();
            }

            var message = TimeBasedCache.Get(identifier);

            RemoveMessageFromCache(identifier, message);

            return message;
        }
    }
}