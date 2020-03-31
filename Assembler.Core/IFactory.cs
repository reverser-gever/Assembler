using Assembler.Core.Entities;

namespace Assembler.Core
{
    public interface IIdentifierGenerator<in T>
        where T : BaseFrame
    {
        string Create(T input);
    }
}