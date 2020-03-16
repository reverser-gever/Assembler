using Assembler.Core;
using Assembler.Core.Abstractions;

namespace Assembler.Base
{
    public abstract class BaseInitialMessageHandler<TFrame, TMessage> : IHandler
        where TFrame : BaseFrame
        where TMessage : BaseAssembledMessage
    {
        private readonly ITimeBasedCache<TMessage> _cache;
        private readonly IFactory<TFrame, string> _identifierFactory;
        private readonly ICreator<TMessage> _assembledMessageCreator;
        private readonly ILogger _logger;

        protected BaseInitialMessageHandler(ITimeBasedCache<TMessage> cache,
            IFactory<TFrame, string> identifierFactory, ICreator<TMessage> assembledMessageCreator,
            ILoggerFactory loggerFactory)
        {
            _cache = cache;
            _identifierFactory = identifierFactory;
            _assembledMessageCreator = assembledMessageCreator;
            _logger = loggerFactory.GetLogger(this);
        }

        public void Handle(BaseFrame frame)
        {
            var castFrame = frame as TFrame;

            var identifier = _identifierFactory.Create(castFrame);

            TMessage message = _cache.Exists(identifier)
               ? _cache.Get<TMessage>(identifier)
               : _assembledMessageCreator.Create();

            EnrichMessage(castFrame, message);

            _cache.Put(identifier, message);
        }

        protected abstract void EnrichMessage(TFrame frame, TMessage message);
    }
}