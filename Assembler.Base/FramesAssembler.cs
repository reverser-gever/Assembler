using System;
using System.Collections.Generic;
using System.Linq;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base
{
    public class FramesAssembler<TFrame, TAssembledMessage> : IAssembler<TFrame, TAssembledMessage>
        where TFrame : BaseFrame
        where TAssembledMessage : BaseAssembledMessage
    {
        private readonly IResolver<FrameType, IFrameHandler<TFrame, BaseMessageInAssembly>> _typeToHandlerResolver;
        private readonly IConverter<BaseMessageInAssembly, TAssembledMessage> _messageInAssemblyToAssembledMessageConverter;
        private readonly ILogger _logger;

        public event Action<TAssembledMessage> MessageAssembled;

        public FramesAssembler(
            IResolver<FrameType, IFrameHandler<TFrame, BaseMessageInAssembly>> typeToHandlerResolver,
            ITimeBasedCache<BaseMessageInAssembly> cache, IEnumerable<IFrameHandler<TFrame, BaseMessageInAssembly>> handlers,
            IConverter<BaseMessageInAssembly, TAssembledMessage> messageInAssemblyToAssembledMessageConverter,
            ILoggerFactory loggerFactory)
        {
            _typeToHandlerResolver = typeToHandlerResolver;
            _messageInAssemblyToAssembledMessageConverter = messageInAssemblyToAssembledMessageConverter;
            _logger = loggerFactory.GetLogger(this);

            SubscribeToEvents(cache, handlers);
        }

        public void Assemble(TFrame frame)
        {
            IFrameHandler<TFrame, BaseMessageInAssembly> handler = _typeToHandlerResolver.Resolve(frame.FrameType);
            handler.Handle(frame);
        }

        private void ReleaseExpiredMessage(BaseMessageInAssembly message)
        {
            ReleaseMessage(message, ReleaseReason.TimeoutReached);
        }

        private void ReleaseMessage(BaseMessageInAssembly message, ReleaseReason releaseReason)
        {
            _logger.Info($"The message [{message.Guid}] was released, the reason for it is [{releaseReason}]");

            var assembledMessage = _messageInAssemblyToAssembledMessageConverter.Convert(message);

            MessageAssembled?.Invoke(assembledMessage);
        }

        private void SubscribeToEvents(ITimeBasedCache<BaseMessageInAssembly> cache,
            IEnumerable<IFrameHandler<TFrame, BaseMessageInAssembly>> handlers)
        {
            cache.ItemExpired += ReleaseExpiredMessage;

            if (!(handlers ?? throw new ArgumentNullException(nameof(handlers))).Any())
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