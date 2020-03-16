using System;
using Assembler.Core.Abstractions;

namespace Assembler.Core
{
    public interface IAssembler
    {
        void Assemble(BaseFrame message);

        event Action<BaseAssembledMessage> OnItemAssembled;
    }
}