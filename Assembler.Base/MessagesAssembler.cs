using System;
using System.Collections.Generic;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base
{
    public class MessagesAssembler : IAssembler
    {
        private readonly IResolver<FrameType, IFrameHandler<BaseFrame, BaseMessageInAssembly>> _typeToHandlerResolver;
        private readonly IConverter<BaseMessageInAssembly, BaseAssembledMessage> _messageInAssemblyToAssembledMessageConverter;
        private readonly ILogger _logger;

        public event Action<BaseAssembledMessage> MessageAssembled;

        public MessagesAssembler(
            IResolver<FrameType, IFrameHandler<BaseFrame, BaseMessageInAssembly>> typeToHandlerResolver,
            ITimeBasedCache<BaseMessageInAssembly> cache, IEnumerable<IFrameHandler<BaseFrame, BaseMessageInAssembly>> handlers,
            IConverter<BaseMessageInAssembly, BaseAssembledMessage> messageInAssemblyToAssembledMessageConverter,
            ILoggerFactory loggerFactory)
        {
            _typeToHandlerResolver = typeToHandlerResolver;
            _messageInAssemblyToAssembledMessageConverter = messageInAssemblyToAssembledMessageConverter;
            _logger = loggerFactory.GetLogger(this);

            cache.ItemExpired += ReleaseExpiredMessage;

            foreach (var handler in handlers)
            {
                handler.MessageAssemblyFinished += ReleaseMessage;
            }
        }

        public void Assemble(BaseFrame message)
        {
            IFrameHandler<BaseFrame, BaseMessageInAssembly> handler = _typeToHandlerResolver.Resolve(message.FrameType);
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

            var assembledMessage = _messageInAssemblyToAssembledMessageConverter.Convert(message);

            MessageAssembled?.Invoke(assembledMessage);
        }
    }
}