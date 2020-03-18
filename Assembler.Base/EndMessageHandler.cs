using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base
{
    public class EndMessageHandler<TFrame, TMessage> : BaseMessageHandler<TFrame, TMessage>
        where TFrame : BaseFrame
        where TMessage : BaseMessageInAssembly
    {
        private readonly bool _isToReleaseOnlyEndFrame;
        private readonly ILogger _logger;

        public EndMessageHandler(ITimeBasedCache<TMessage> cache,
            IFactory<TFrame, string> identifierFactory, IMessageEnricher<TFrame, TMessage> enricher, bool isToReleaseOnlyEndFrame,
            ILoggerFactory loggerFactory) : base(cache, identifierFactory, enricher)
        {
            _isToReleaseOnlyEndFrame = isToReleaseOnlyEndFrame;
            _logger = loggerFactory.GetLogger(this);
        }

        public override void Handle(BaseFrame frame)
        {
            var castFrame = frame as TFrame;

            var identifier = GetIdentifier(castFrame);

            TMessage message;

            if (!Cache.Exists(identifier))
            {
                if (!_isToReleaseOnlyEndFrame)
                {
                    _logger.Debug(
                        $"An end message [{frame.Guid}] was received, " +
                        "but there where no open message in assembly with the same identifier. " +
                        "It will not be used.");
                    return;
                }

                message = CreateMessage();
            }
            else
            {
                message = Cache.Get<TMessage>(identifier);
                Cache.Remove(identifier);
            }

            MessageEnricher.Enrich(castFrame, message);

            _logger.Debug(
                $"Enriched [{message.Guid}] with the frame [{frame.Guid}]. It was removed from the cache.");

            message.ReleaseReason = ReleaseReason.EndReceived;
            ReleaseMessage(message);
        }
    }
}