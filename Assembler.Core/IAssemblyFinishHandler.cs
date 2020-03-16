using System;
using Assembler.Core.Abstractions;

namespace Assembler.Core
{
    public interface IAssemblyFinishHandler : IHandler
    {
        event Action<BaseAssembledMessage> OnMessageAssembled;
    }
}