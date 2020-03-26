using System;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Core
{
    public interface IMessageReleaser<TMessageInAssembly>
        where TMessageInAssembly : BaseMessageInAssembly
    {
        void Release(TMessageInAssembly message, ReleaseReason releaseReason);

        event Action<TMessageInAssembly> MessageReleased;
    }
}