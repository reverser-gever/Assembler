using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Releasing;
using Microsoft.Extensions.Logging;

namespace Assembler.Base.FrameHandlers
{
    public class MiddleFrameHandler<TFrame, TMessageInAssembly> : BaseFrameHandler<TFrame, TMessageInAssembly>
        where TFrame : BaseFrame
        where TMessageInAssembly : BaseMessageInAssembly
    {
        public MiddleFrameHandler(ITimeBasedCache<TMessageInAssembly> timeBasedCache,
            IIdentifierGenerator<TFrame> identifierGenerator,
            IMessageInAssemblyCreator<TMessageInAssembly> messageInAssemblyCtrCreator,
            IMessageEnricher<TFrame, TMessageInAssembly> messageInAssemblyEnricher,
            IMessageInAssemblyReleaser<TMessageInAssembly> messageInAssemblyReleaser, IDateTimeProvider dateTimeProvider,
            ILoggerFactory loggerFactory)
            : base(timeBasedCache, identifierGenerator, messageInAssemblyEnricher, messageInAssemblyCtrCreator,
                messageInAssemblyReleaser, dateTimeProvider, loggerFactory)
        { }

        public override void Handle(TFrame frame)
        {
            if (!TryGetIdentifier(frame, out var identifier)) return;

            TMessageInAssembly message = GetOrCreateMessageInAssembly(identifier);

            EnrichMessage(frame, message);

            message.MiddleReceived = true;

            TimeBasedCache.Put(identifier, message);
        }

        private TMessageInAssembly GetOrCreateMessageInAssembly(string identifier)
        {
            if (TimeBasedCache.Exists(identifier))
            {
                return TimeBasedCache.Get(identifier);
            }

            LogNoMessageInCache(identifier);
            var message = CreateMessage();

            return message;
        }
    }
}