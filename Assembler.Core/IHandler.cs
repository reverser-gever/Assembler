﻿using Assembler.Core.Entities;

namespace Assembler.Core
{
    public interface IHandler
    {
        void Handle(BaseFrame message);
    }
}