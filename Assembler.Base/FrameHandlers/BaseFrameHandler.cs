using System;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Microsoft.Extensions.Logging;

namespace Assembler.Base.FrameHandlers
{
    public abstract class BaseFrameHandler<TFrame, TMessageInAssembly> : IFrameHandler<TFrame>
        where TFrame : BaseFrame
        where TMessageInAssembly : BaseMessageInAssembly
    {
        private readonly IIdentifierGenerator<TFrame> _identifierGenerator;
        private readonly IMessageEnricher<TFrame, TMessageInAssembly> _messageInAssemblyEnricher;
        private readonly IMessageInAssemblyCreator<TMessageInAssembly> _messageInAssemblyCreator;
        private readonly IMessageReleaser<TMessageInAssembly> _messageInAssemblyReleaser;
        private readonly IDateTimeProvider _dateTimeProvider;

        protected readonly ITimeBasedCache<TMessageInAssembly> TimeBasedCache;
        protected readonly ILogger Logger;

        protected BaseFrameHandler(ITimeBasedCache<TMessageInAssembly> timeBasedCache,
            IIdentifierGenerator<TFrame> identifierGenerator,
            IMessageEnricher<TFrame, TMessageInAssembly> messageInAssemblyEnricher,
            IMessageInAssemblyCreator<TMessageInAssembly> messageInAssemblyCreator,
            IMessageReleaser<TMessageInAssembly> messageInAssemblyReleaser,
            IDateTimeProvider dateTimeProvider, ILoggerFactory loggerFactory)
        {
            TimeBasedCache = timeBasedCache;
            _identifierGenerator = identifierGenerator;
            _messageInAssemblyEnricher = messageInAssemblyEnricher;
            _messageInAssemblyCreator = messageInAssemblyCreator;
            _messageInAssemblyReleaser = messageInAssemblyReleaser;
            _dateTimeProvider = dateTimeProvider;
            Logger = loggerFactory.CreateLogger(ToString());
        }

        public abstract void Handle(TFrame frame);

        protected bool TryGetIdentifier(TFrame frame, out string identifier)
        {
            try
            {
                identifier = _identifierGenerator.Create(frame);
            }
            catch (Exception e)
            {
                Logger.LogError($"There was an error while getting an identifier out of the frame [{frame.Guid}], " +
                                $"it won't be used it the assembling process \n {e}");

                identifier = null;
                return false;
            }

            return true;
        }

        protected void LogNoMessageInCache(string identifier) =>
            Logger.LogDebug($"No message in cache with the provided identifier [{identifier}]");

        protected void RemoveMessageFromCache(string identifier, TMessageInAssembly message)
        {
            TimeBasedCache.Remove(identifier);
            Logger.LogDebug(
                $"The message [{message.Guid}] with the identifier [{identifier}] was removed from the cache, it will be released.");
        }

        protected TMessageInAssembly CreateMessage()
        {
            var message = _messageInAssemblyCreator.Create();

            Logger.LogDebug($"A new message was created [{message.Guid}].");

            return message;
        }

        protected void ReleaseMessage(TMessageInAssembly message, ReleaseReason releaseReason) =>
            _messageInAssemblyReleaser.Release(message, releaseReason);

        protected void EnrichMessage(TFrame frame, TMessageInAssembly messageInAssembly)
        {
            _messageInAssemblyEnricher.Enrich(frame, messageInAssembly);
            messageInAssembly.LastFrameReceived = _dateTimeProvider.Now;

            Logger.LogDebug(
                $"Enriched [{messageInAssembly.Guid}] with the frame [{frame.Guid}].");
        }
    }
}