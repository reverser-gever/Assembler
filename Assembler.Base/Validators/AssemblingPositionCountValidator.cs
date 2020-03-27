using Assembler.Core;
using Assembler.Core.RawAssemblingEntities;

namespace Assembler.Base.Validators
{
    public class AssemblingPositionCountValidator : IValidator<RawMessageInAssembly>
    {
        private readonly int _minimumInitialFramesCount;
        private readonly int _minimumMiddleFramesCount;
        private readonly int _minimumFinalFramesCount;

        public AssemblingPositionCountValidator(int minimumInitialFramesCount, int minimumMiddleFramesCount, int minimumFinalFramesCount)
        {
            _minimumInitialFramesCount = minimumInitialFramesCount;
            _minimumMiddleFramesCount = minimumMiddleFramesCount;
            _minimumFinalFramesCount = minimumFinalFramesCount;
        }

        public bool IsValid(RawMessageInAssembly rawMessageInAssembly) =>
            rawMessageInAssembly.InitialFrames.Count >= _minimumInitialFramesCount
            && rawMessageInAssembly.MiddleFrames.Count >= _minimumMiddleFramesCount
            && rawMessageInAssembly.FinalFrames.Count >= _minimumFinalFramesCount;
    }
}