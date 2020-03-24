using System;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base
{
    public class EndFrameHandler<TFrame, TMessage> : BaseFrameHandler<TFrame, TMessage>
        where TFrame : BaseFrame
        where TMessage : BaseMessageInAssembly
    {
        private readonly bool _isToReleaseSingleEndFrame;
        private readonly ILogger _logger;

        public EndFrameHandler(ITimeBasedCache<TMessage> cache,
            IFactory<TFrame, string> identifierFactory, IMessageEnricher<TFrame, TMessage> enricher,
            ICreator<TMessage> assembledMessageCreator, bool isToReleaseOnlyEndFrame,
            ILoggerFactory loggerFactory) : base(cache, identifierFactory, enricher, assembledMessageCreator)
        {
            _isToReleaseSingleEndFrame = isToReleaseOnlyEndFrame;
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

            if (message != null)
            {
                MessageEnricher.Enrich(frame, message);

                _logger.Debug(
                    $"Enriched [{message.Guid}] with the frame [{frame.Guid}].");

                message.ReleaseReason = ReleaseReason.EndReceived;
                ReleaseMessage(message);
            }
            else
            {
                _logger.Debug(
                    $"An end message [{frame.Guid}] was received, " +
                    "but there where no open message in assembly with the same identifier. " +
                    "It will not be used.");
            }
        }

        private TMessage GetOrCreateMessageInAssembly(string identifier)
        {
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
                    return null;
                }

                message = CreateMessage();

                _logger.Debug($"A new message was created [{message.Guid}].");
            }

            return message;
        }
    }
}