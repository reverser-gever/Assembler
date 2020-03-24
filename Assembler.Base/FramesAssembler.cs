using System;
using System.Collections.Generic;
using System.Linq;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base
{
    public class FramesAssembler : IAssembler
    {
        private readonly IResolver<FrameType, IFrameHandler<BaseFrame, BaseMessageInAssembly>> _typeToHandlerResolver;
        private readonly IConverter<BaseMessageInAssembly, BaseAssembledMessage> _messageInAssemblyToAssembledMessageConverter;
        private readonly ILogger _logger;

        public event Action<BaseAssembledMessage> MessageAssembled;

        public FramesAssembler(
            IResolver<FrameType, IFrameHandler<BaseFrame, BaseMessageInAssembly>> typeToHandlerResolver,
            ITimeBasedCache<BaseMessageInAssembly> cache, IEnumerable<IFrameHandler<BaseFrame, BaseMessageInAssembly>> handlers,
            IConverter<BaseMessageInAssembly, BaseAssembledMessage> messageInAssemblyToAssembledMessageConverter,
            ILoggerFactory loggerFactory)
        {
            _typeToHandlerResolver = typeToHandlerResolver;
            _messageInAssemblyToAssembledMessageConverter = messageInAssemblyToAssembledMessageConverter;
            _logger = loggerFactory.GetLogger(this);

            SubscribeToEvents(cache, handlers);
        }

        public void Assemble(BaseFrame frame)
        {
            IFrameHandler<BaseFrame, BaseMessageInAssembly> handler = _typeToHandlerResolver.Resolve(frame.FrameType);
            handler.Handle(frame);
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

        private void SubscribeToEvents(ITimeBasedCache<BaseMessageInAssembly> cache,
            IEnumerable<IFrameHandler<BaseFrame, BaseMessageInAssembly>> handlers)
        {
            cache.ItemExpired += ReleaseExpiredMessage;

            if ((handlers ?? throw new ArgumentNullException(nameof(handlers))).ToArray().Length == 0)
            {
                throw new ArgumentException("Handlers can't be null, please add all of the handlers that you use",
                    nameof(handlers));
            }

            foreach (var handler in handlers)
            {
                handler.MessageAssemblyFinished += ReleaseMessage;
            }
        }
    }
}