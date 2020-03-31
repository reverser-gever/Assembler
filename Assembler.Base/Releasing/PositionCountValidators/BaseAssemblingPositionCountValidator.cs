using System.Collections.Generic;
using System.Linq;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.RawAssemblingEntities;
using Microsoft.Extensions.Logging;

namespace Assembler.Base.Releasing.PositionCountValidators
{
    public abstract class BaseAssemblingPositionCountValidator : IValidator<RawMessageInAssembly>
    {
        private readonly int _minimumFramesCount;
        private readonly ILogger _logger;

        protected BaseAssemblingPositionCountValidator(int minimumFramesCount, ILoggerFactory loggerFactory)
        {
            _minimumFramesCount = minimumFramesCount;
            _logger = loggerFactory.CreateLogger(ToString());
        }

        public bool IsValid(RawMessageInAssembly message)
        {
            var framesList = GetFramesList(message).ToArray();

            if (framesList.Length >= _minimumFramesCount)
            {
                _logger.LogDebug($"The message [{message.Guid}] passed the validator");
                return true;
            }

            _logger.LogDebug($"The message [{message.Guid}] failed the validator");
            return false;
        }

        protected abstract IEnumerable<BaseFrame> GetFramesList(RawMessageInAssembly message);
    }
}