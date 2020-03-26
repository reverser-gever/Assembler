using System;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base
{
    public class MessageReleaser<TMessageInAssembly> : IMessageReleaser<TMessageInAssembly>
        where TMessageInAssembly : BaseMessageInAssembly
    {
        private readonly ILogger _logger;

        public event Action<TMessageInAssembly> MessageReleased;

        public MessageReleaser(ITimeBasedCache<TMessageInAssembly> timeBasedCache,
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLogger(this);

            timeBasedCache.ItemExpired += ReleaseExpiredMessage;
        }

        public void Release(TMessageInAssembly message, ReleaseReason releaseReason)
        {
            _logger.Info($"The message [{message.Guid}] was released, the reason for it is [{releaseReason}]");

            MessageReleased?.Invoke(message);
        }

        private void ReleaseExpiredMessage(TMessageInAssembly message)
        {
            Release(message, ReleaseReason.TimeoutReached);
        }
    }
}