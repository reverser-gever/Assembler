using System;
using Assembler.Core;
using Assembler.Core.Entities;

namespace Assembler.Base
{
    public class MiddleFrameHandler<TFrame, TMessageInAssembly> : BaseFrameHandler<TFrame, TMessageInAssembly>
        where TFrame : BaseFrame
        where TMessageInAssembly : BaseMessageInAssembly
    {
        private readonly ILogger _logger;

        public MiddleFrameHandler(ITimeBasedCache<TMessageInAssembly> timeBasedCache,
            IFactory<TFrame, string> identifierFactory, ICreator<TMessageInAssembly> messageInAssemblyCtrCreator,
            IMessageEnricher<TFrame, TMessageInAssembly> messageInAssemblyEnricher,
            IMessageReleaser<TMessageInAssembly> messageReleaser, ILoggerFactory loggerFactory)
            : base(timeBasedCache, identifierFactory, messageInAssemblyEnricher, messageInAssemblyCtrCreator,
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
            if (TimeBasedCache.Exists(identifier))
            {
                return TimeBasedCache.Get<TMessageInAssembly>(identifier);
            }

            var message = CreateMessage();
            message.MiddleReceived = true;

            _logger.Debug(
                $"No message in cache with the expected identifier, created a new message [{message.Guid}]");

            return message;
        }
    }
}