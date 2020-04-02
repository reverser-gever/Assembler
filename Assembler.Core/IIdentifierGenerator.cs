using Assembler.Core.Entities;

namespace Assembler.Core
{
    public interface IIdentifierGenerator<in TFrame>
        where TFrame : BaseFrame
    {
        string Generate(TFrame input);
    }
}