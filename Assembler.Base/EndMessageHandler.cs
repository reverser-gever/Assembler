using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base
{
    public class EndMessageHandler<TFrame, TMessage> : BaseMessageHandler<TFrame, TMessage>
        where TFrame : BaseFrame
        where TMessage : BaseMessageInAssembly
    {
        private readonly bool _isToReleaseSingleEndFrame;
        private readonly ILogger _logger;

        public EndMessageHandler(ITimeBasedCache<TMessage> cache,
            IFactory<TFrame, string> identifierFactory, IMessageEnricher<TFrame, TMessage> enricher,
            ICreator<TMessage> assembledMessageCreator, bool isToReleaseOnlyEndFrame,
            ILoggerFactory loggerFactory) : base(cache, identifierFactory, enricher, assembledMessageCreator)
        {
            _isToReleaseSingleEndFrame = isToReleaseOnlyEndFrame;
            _logger = loggerFactory.GetLogger(this);
        }

        public override void Handle(TFrame frame)
        {
            var identifier = GetIdentifier(frame);

            TMessage message;

            if (Cache.Exists(identifier))
            {
                message = Cache.Get<TMessage>(identifier);
                Cache.Remove(identifier);

                _logger.Debug(
                    $"The message [{message.Guid}] was removed from the cache, it's set for release.");
            }
            else
            {
                if (!_isToReleaseSingleEndFrame)
                {
                    _logger.Debug(
                        $"An end message [{frame.Guid}] was received, " +
                        "but there where no open message in assembly with the same identifier. " +
                        "It will not be used.");
                    return;
                }

                message = CreateMessage();

                _logger.Debug($"A new message was created [{message.Guid}].");
            }

            MessageEnricher.Enrich(frame, message);

            _logger.Debug(
                $"Enriched [{message.Guid}] with the frame [{frame.Guid}].");

            message.ReleaseReason = ReleaseReason.EndReceived;
            ReleaseMessage(message);
        }
    }
}