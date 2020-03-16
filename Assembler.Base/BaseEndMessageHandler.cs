using System;
using Assembler.Core;
using Assembler.Core.Abstractions;
using Assembler.Core.Enums;

namespace Assembler.Base
{
    public abstract class BaseEndMessageHandler<TFrame, TMessage> : BaseHandler<TFrame, TMessage>, IAssemblyFinishHandler
        where TFrame : BaseFrame
        where TMessage : BaseAssembledMessage
    {
        private readonly ILogger _logger;

        public event Action<BaseAssembledMessage> OnMessageAssembled;

        protected BaseEndMessageHandler(ITimeBasedCache<TMessage> cache,
            IFactory<TFrame, string> identifierFactory, ILoggerFactory loggerFactory) : base(cache, identifierFactory)
        {
            _logger = loggerFactory.GetLogger(this);
        }

        public override void Handle(BaseFrame frame)
        {
            var castFrame = frame as TFrame;

            var identifier = GetIdentifier(castFrame);

            if (!Cache.Exists(identifier))
            {
                _logger.Debug(
                    $"An end message [{frame.Guid}] was received, " +
                    "but there where no open message in assembly with the same identifier. " +
                    "It will not be used.");
                return;
            }

            var message = Cache.Get<TMessage>(identifier);
            Cache.Remove(identifier);

            EnrichMessageWithFrame(castFrame, message);

            _logger.Debug(
                $"Enriched [{message.Guid}] with the frame [{frame.Guid}]. It was removed from the cache.");

            message.ReleaseReason = ReleaseReason.EndReceived;
            OnMessageAssembled?.Invoke(message);
        }
    }
}