using Assembler.Core.Entities;

namespace Assembler.Core
{
    public interface IAssembler<in TFrame>
        where TFrame : BaseFrame
    {
        void Assemble(TFrame message);
    }
}