using Assembler.Core;
using Assembler.Core.Entities;

namespace Assembler.Base
{
    public abstract class BaseMiddleMessageHandler<TFrame, TMessage> : BaseMessageHandler<TFrame, TMessage>
        where TFrame : BaseFrame
        where TMessage : BaseMessageInAssembly
    {
        private readonly ICreator<TMessage> _assembledMessageCreator;
        private readonly ILogger _logger;

        protected BaseMiddleMessageHandler(ITimeBasedCache<TMessage> cache,
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
            }
            else
            {
                // We have a few options here, if we get a start message after that what do we do?
                // Do we assume the order we got is wrong and keeping [MiddleReceived] as false?
                // That way if we get a start message after this one we keep assembling as if it was the same message.
                // If we set it to true we would create a new message if we get another start.

                message = _assembledMessageCreator.Create();
                message.MiddleReceived = true;

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