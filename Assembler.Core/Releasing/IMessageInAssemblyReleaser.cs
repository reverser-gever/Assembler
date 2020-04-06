using System;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Core.Releasing
{
    public interface IMessageInAssemblyReleaser<TMessageInAssembly> : IStartable
        where TMessageInAssembly : BaseMessageInAssembly
    {
        void Release(TMessageInAssembly message, ReleaseReason releaseReason);

        event Action<TMessageInAssembly> MessageReleased;
    }
}