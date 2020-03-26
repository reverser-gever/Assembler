using System;
using Assembler.Core.Entities;

namespace Assembler.Core
{
    public interface IAssembler<in TFrame, out TAssembledMessage>
        where TFrame : BaseFrame
        where TAssembledMessage : BaseAssembledMessage
    {
        void Assemble(TFrame message);

        event Action<TAssembledMessage> MessageAssembled;
    }
}