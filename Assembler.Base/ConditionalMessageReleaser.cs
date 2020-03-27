using System;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base
{
    public class ConditionalMessageReleaser<TMessageInAssembly> : MessageReleaser<TMessageInAssembly>
        where TMessageInAssembly : BaseMessageInAssembly
    {
        private readonly IValidator<TMessageInAssembly> _messageValidator;

        public ConditionalMessageReleaser(IValidator<TMessageInAssembly> messageValidator,
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
        }
    }
}