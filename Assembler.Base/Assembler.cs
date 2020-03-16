using System;
using System.Collections.Generic;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base
{
    public class Assembler : IAssembler
    {
        private readonly IResolver<MessageType, IHandler> _resolver;
        private readonly IConverter<BaseMessageInAssembly, BaseAssembledMessage> _converter;
        private readonly ILogger _logger;

        public event Action<BaseAssembledMessage> OnMessageAssembled;

        public Assembler(IResolver<MessageType, IHandler> resolver,
            ITimeBasedCache<BaseMessageInAssembly> cache,
            IEnumerable<IHandler> handlers, IConverter<BaseMessageInAssembly, BaseAssembledMessage> converter,
            ILoggerFactory loggerFactory)
        {
            _resolver = resolver;
            _converter = converter;
            _logger = loggerFactory.GetLogger(this);

            cache.OnItemExpired += ReleaseExpiredMessage;

            foreach (var handler in handlers)
            {
                handler.OnMessageFinishedAssembly += ReleaseMessage;
            }
        }

        public void Assemble(BaseFrame message)
        {
            var handler = _resolver.Resolve(message.MessageType);
            handler.Handle(message);
        }

        private void ReleaseExpiredMessage(BaseMessageInAssembly message)
        {
            message.ReleaseReason = ReleaseReason.TimeoutReached;
            ReleaseMessage(message);
        }

        private void ReleaseMessage(BaseMessageInAssembly message)
        {
            _logger.Info($"The message [{message.Guid}] was released, the reason for it is [{message.ReleaseReason}]");

            var assembledMessage = _converter.Convert(message);

            OnMessageAssembled?.Invoke(assembledMessage);
        }
    }
}