﻿using System;
using Assembler.Core.Entities;

namespace Assembler.Core
{
    public interface IHandler<in TFrame, out TMessage>
        where TFrame : BaseFrame
        where TMessage : BaseMessageInAssembly
    {
        void Handle(TFrame message);

        event Action<TMessage> MessageAssemblyFinished;
    }
}