using System;
using Assembler.Core.Abstractions;

namespace Assembler.Core
{
    public interface IAssembler
    {
        // Use T where T : BaseFrame too here?
        void Assemble(BaseFrame message);

        event Action<BaseAssembledMessage> OnItemAssembled;
    }
}