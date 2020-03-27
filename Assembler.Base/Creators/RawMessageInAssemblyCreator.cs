using Assembler.Core;
using Assembler.Core.RawAssemblingEntities;

namespace Assembler.Base.Creators
{
    public class RawMessageInAssemblyCreator : ICreator<RawMessageInAssembly>
    {
        public RawMessageInAssembly Create() => new RawMessageInAssembly();
    }
}