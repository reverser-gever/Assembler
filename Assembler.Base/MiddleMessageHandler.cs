using Assembler.Core;
using Assembler.Core.Entities;

namespace Assembler.Base
{
    public class MiddleMessageHandler<TFrame, TMessage> : BaseMessageHandler<TFrame, TMessage>
        where TFrame : BaseFrame
        where TMessage : BaseMessageInAssembly
    {
        private readonly ILogger _logger;

        public MiddleMessageHandler(ITimeBasedCache<TMessage> cache,
            IFactory<TFrame, string> identifierFactory, ICreator<TMessage> assembledMessageCreator,
            IMessageEnricher<TFrame, TMessage> enricher, ILoggerFactory loggerFactory) : base(cache, identifierFactory,
            enricher, assembledMessageCreator)
        {
            _logger = loggerFactory.GetLogger(this);
        }

        public override void Handle(TFrame frame)
        {
            var identifier = GetIdentifier(frame);

            TMessage message;

            if (Cache.Exists(identifier))
            {
                message = Cache.Get<TMessage>(identifier);
            }
            else
            {
                // We have a few options here, if we get a start message after that what do we do?
                // Do we assume the order we got is wrong and keeping [MiddleReceived] as false?
                // That way if we get a start message after this one we keep assembling as if it was the same message.
                // If we set it to true we would create a new message if we get another start.

                message = CreateMessage();
                message.MiddleReceived = true;

                _logger.Debug(
                    $"No message in cache with the expected identifier, creating a new message [{message.Guid}]");
            }

            MessageEnricher.Enrich(frame, message);

            _logger.Debug(
                $"Enriched [{message.Guid}] with the frame [{frame.Guid}] ");

            Cache.Put(identifier, message);
        }
    }
}