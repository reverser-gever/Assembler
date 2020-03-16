using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base
{
    public abstract class BaseInitialMessageHandler<TFrame, TMessage> : BaseHandler<TFrame, TMessage>
        where TFrame : BaseFrame
        where TMessage : BaseMessageInAssembly
    {
        private readonly ICreator<TMessage> _assembledMessageCreator;
        private readonly ILogger _logger;

        protected BaseInitialMessageHandler(ITimeBasedCache<TMessage> cache,
            IFactory<TFrame, string> identifierFactory, ICreator<TMessage> assembledMessageCreator,
            ILoggerFactory loggerFactory) : base(cache, identifierFactory)
        {
            _assembledMessageCreator = assembledMessageCreator;
            _logger = loggerFactory.GetLogger(this);
        }

        public override void Handle(BaseFrame frame)
        {
            var castFrame = frame as TFrame;

            var identifier = GetIdentifier(castFrame);

            TMessage message;

            if (Cache.Exists(identifier))
            {
                message = Cache.Get<TMessage>(identifier);

                // Again we are facing a decision,
                // If we get a start - middle - start, do we suppose there a was a mismatch in the order?
                // I think that we don't, we'd start a new message
                if (message.MiddleReceived)
                {
                    _logger.Debug(
                        "Received another initial frame after started collecting the middle frames." +
                        $"The old message [{message.Guid}] will be released.");

                    Cache.Remove(identifier);

                    message.ReleaseReason = ReleaseReason.AnotherMessageStarted;
                    ReleaseMessage(message);

                    message = _assembledMessageCreator.Create();

                    _logger.Debug($"A new message was created [{message.Guid}]");
                }
            }
            else
            {
                message = _assembledMessageCreator.Create();

                _logger.Debug(
                    $"No message in cache with the expected identifier, creating a new message [{message.Guid}]");
            }

            EnrichMessageWithFrame(castFrame, message);

            _logger.Debug(
                $"Enriched [{message.Guid}] with the frame [{frame.Guid}] ");

            Cache.Put(identifier, message);
        }
    }
}