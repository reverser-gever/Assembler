using System.Collections.Generic;
using Assembler.Core.Entities;
using Assembler.Core.RawAssemblingEntities;
using Microsoft.Extensions.Logging;

namespace Assembler.Base.Releasing.PositionCountValidators
{
    public class MiddleAssemblingPositionCountValidator : BaseAssemblingPositionCountValidator
    {
        public MiddleAssemblingPositionCountValidator(int minimumFramesCount, ILoggerFactory loggerFactory)
            : base(minimumFramesCount, loggerFactory)
        { }

        protected override IEnumerable<BaseFrame> GetFramesList(RawMessageInAssembly message) => message.MiddleFrames;
    }
}