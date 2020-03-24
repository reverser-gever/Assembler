using System;
using Assembler.Core;
using Assembler.Core.Entities;

namespace Assembler.Base
{
    public class MiddleFrameHandler<TFrame, TMessage> : BaseFrameHandler<TFrame, TMessage>
        where TFrame : BaseFrame
        where TMessage : BaseMessageInAssembly
    {
        private readonly ILogger _logger;

        public MiddleFrameHandler(ITimeBasedCache<TMessage> cache,
            IFactory<TFrame, string> identifierFactory, ICreator<TMessage> assembledMessageCreator,
            IMessageEnricher<TFrame, TMessage> enricher, ILoggerFactory loggerFactory) : base(cache, identifierFactory,
            enricher, assembledMessageCreator)
        {
            _logger = loggerFactory.GetLogger(this);
        }

        public override void Handle(TFrame frame)
        {
            string identifier;

            try
            {
                identifier = GetIdentifier(frame);
            }
            catch (Exception e)
            {
                _logger.Error($"There was an error while getting an identifier out of the frame [{frame.Guid}], " +
                              $"it won't be used it the assembling process \n {e}");
                return;
            }

            TMessage message = GetOrCreateMessageInAssembly(identifier);

            MessageEnricher.Enrich(frame, message);

            _logger.Debug(
                $"Enriched [{message.Guid}] with the frame [{frame.Guid}] ");

            Cache.Put(identifier, message);
        }

        private TMessage GetOrCreateMessageInAssembly(string identifier)
        {
            if (Cache.Exists(identifier))
            {
                return Cache.Get<TMessage>(identifier);
            }

            // We have a few options here, if we get a start message after that what do we do?
            // Do we assume the order we got is wrong and keeping [MiddleReceived] as false?
            // That way if we get a start message after this one we keep assembling as if it was the same message.
            // If we set it to true we would create a new message if we get another start.

            var message = CreateMessage();
            message.MiddleReceived = true;

            _logger.Debug(
                $"No message in cache with the expected identifier, created a new message [{message.Guid}]");

            return message;
        }
    }
}