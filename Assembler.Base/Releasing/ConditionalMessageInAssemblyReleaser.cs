using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Microsoft.Extensions.Logging;

namespace Assembler.Base.Releasing
{
    public class ConditionalMessageInAssemblyReleaser<TMessageInAssembly> : MessageInAssemblyReleaser<TMessageInAssembly>
        where TMessageInAssembly : BaseMessageInAssembly
    {
        private readonly IValidator<TMessageInAssembly> _messageValidator;

        public ConditionalMessageInAssemblyReleaser(IValidator<TMessageInAssembly> messageValidator,
            ITimeBasedCache<TMessageInAssembly> timeBasedCache,
            ILoggerFactory loggerFactory) : base(timeBasedCache, loggerFactory)
        {
            _messageValidator = messageValidator;
        }

        public override void Release(TMessageInAssembly message, ReleaseReason releaseReason)
        {
            if (_messageValidator.IsValid(message))
            {
                base.Release(message, releaseReason);
            }
            else
            {
                Logger.LogDebug($"Message [{message.Guid}] with the release reason of [{releaseReason}] " +
                            "didn't pass the validator, it won't be released.");
            }
        }
    }
}