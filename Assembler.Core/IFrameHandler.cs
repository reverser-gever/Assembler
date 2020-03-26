using Assembler.Core.Entities;

namespace Assembler.Core
{
    public interface IFrameHandler<in TFrame>
        where TFrame : BaseFrame
    {
        void Handle(TFrame message);
    }
}