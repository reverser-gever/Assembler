using Assembler.Core;
using Assembler.Core.Entities;

namespace Assembler.Base
{
    public abstract class BaseInitialMessageHandler<TFrame, TMessage> : BaseHandler<TFrame, TMessage>
        where TFrame : BaseFrame
        where TMessage : BaseAssembledMessage
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