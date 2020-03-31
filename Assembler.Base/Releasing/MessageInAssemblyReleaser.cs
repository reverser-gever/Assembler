using System;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Microsoft.Extensions.Logging;

namespace Assembler.Base.Releasing
{
    public class MessageInAssemblyReleaser<TMessageInAssembly> : IMessageReleaser<TMessageInAssembly>
        where TMessageInAssembly : BaseMessageInAssembly
    {
        private readonly ITimeBasedCache<TMessageInAssembly> _timeBasedCache;

        protected readonly ILogger Logger;

        public event Action<TMessageInAssembly> MessageReleased;

        public MessageInAssemblyReleaser(ITimeBasedCache<TMessageInAssembly> timeBasedCache, ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger(ToString());
            _timeBasedCache = timeBasedCache;
        }

        public void Start()
        {
            _timeBasedCache.ItemExpired += ReleaseExpiredMessage;
        }

        public void Dispose()
        {
            _timeBasedCache.ItemExpired -= ReleaseExpiredMessage;
        }

        public virtual void Release(TMessageInAssembly message, ReleaseReason releaseReason)
        {
            Logger.LogDebug($"The message [{message.Guid}] was released, the reason for it is [{releaseReason}]");

            message.ReleaseReason = releaseReason;

            MessageReleased?.Invoke(message);
        }

        private void ReleaseExpiredMessage(TMessageInAssembly message)
        {
            Release(message, ReleaseReason.TimeoutReached);
        }
    }
}