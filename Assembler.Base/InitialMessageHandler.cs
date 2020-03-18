using System;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base
{
    public class InitialMessageHandler<TFrame, TMessage> : BaseMessageHandler<TFrame, TMessage>
        where TFrame : BaseFrame
        where TMessage : BaseMessageInAssembly
    {
        private readonly ILogger _logger;

        public InitialMessageHandler(ITimeBasedCache<TMessage> cache,
            IFactory<TFrame, string> identifierFactory, ICreator<TMessage> assembledMessageCreator,
            IMessageEnricher<TFrame, TMessage> enricher, ILoggerFactory loggerFactory) : base(cache,
            identifierFactory, enricher, assembledMessageCreator)
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

                    message = CreateMessage();

                    _logger.Debug($"A new message was created [{message.Guid}]");
                }
            }
            else
            {
                message = CreateMessage();

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