using System;
using Assembler.Core.Entities;

namespace Assembler.Core
{
    public interface IAssemblyFinishHandler : IHandler
    {
        event Action<BaseMessageInAssembly> OnMessageAssembled;
    }
}