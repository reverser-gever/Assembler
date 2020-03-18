using System;
using System.Collections.Generic;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base
{
    public class MessagesAssembler : IAssembler
    {
        private readonly IResolver<FrameType, IHandler<BaseFrame, BaseMessageInAssembly>> _resolver;
        private readonly IConverter<BaseMessageInAssembly, BaseAssembledMessage> _converter;
        private readonly ILogger _logger;

        public event Action<BaseAssembledMessage> MessageAssembled;

        public MessagesAssembler(IResolver<FrameType, IHandler<BaseFrame, BaseMessageInAssembly>> resolver,
            ITimeBasedCache<BaseMessageInAssembly> cache,
            IEnumerable<IHandler<BaseFrame, BaseMessageInAssembly>> handlers, IConverter<BaseMessageInAssembly, BaseAssembledMessage> converter,
            ILoggerFactory loggerFactory)
        {
            _resolver = resolver;
            _converter = converter;
            _logger = loggerFactory.GetLogger(this);

            cache.ItemExpired += ReleaseExpiredMessage;

            foreach (var handler in handlers)
            {
                handler.MessageAssemblyFinished += ReleaseMessage;
            }
        }

        public void Assemble(BaseFrame message)
        {
            var handler = _resolver.Resolve(message.FrameType);
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

            MessageAssembled?.Invoke(assembledMessage);
        }
    }
}