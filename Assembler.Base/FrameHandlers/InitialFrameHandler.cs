using System;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base.FrameHandlers
{
    public class InitialFrameHandler<TFrame, TMessageInAssembly> : BaseFrameHandler<TFrame, TMessageInAssembly>
        where TFrame : BaseFrame
        where TMessageInAssembly : BaseMessageInAssembly
    {
        private readonly ILogger _logger;

        public InitialFrameHandler(ITimeBasedCache<TMessageInAssembly> timeBasedCache,
            IFactory<TFrame, string> identifierFactory, ICreator<TMessageInAssembly> messageInAssemblyCreator,
            IMessageEnricher<TFrame, TMessageInAssembly> messageInAssemblyEnricher,
            IMessageReleaser<TMessageInAssembly> messageReleaser, ILoggerFactory loggerFactory)
            : base(timeBasedCache, identifierFactory, messageInAssemblyEnricher, messageInAssemblyCreator,
                messageReleaser)
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

            TMessageInAssembly message = GetOrCreateMessageInAssembly(identifier);

            MessageInAssemblyEnricher.Enrich(frame, message);

            _logger.Debug(
                $"Enriched [{message.Guid}] with the frame [{frame.Guid}] ");

            TimeBasedCache.Put(identifier, message);
        }

        private TMessageInAssembly GetOrCreateMessageInAssembly(string identifier)
        {
            TMessageInAssembly message;

            if (!TimeBasedCache.Exists(identifier))
            {
                message = CreateMessage();

                _logger.Debug(
                    $"No message in cache with the expected identifier, creating a new message [{message.Guid}]");

                return message;
            }

            message = TimeBasedCache.Get<TMessageInAssembly>(identifier);

            if (message.MiddleReceived)
            {
                _logger.Debug(
                    "Received another initial frame after started collecting the middle frames." +
                    $"The old message [{message.Guid}] will be released.");

                TimeBasedCache.Remove(identifier);

                ReleaseMessage(message, ReleaseReason.AnotherMessageStarted);

                message = CreateMessage();

                _logger.Debug($"A new message was created [{message.Guid}]");
            }

            return message;
        }
    }
}