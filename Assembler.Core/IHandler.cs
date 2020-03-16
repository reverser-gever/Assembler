using System;
using Assembler.Core.Abstractions;

namespace Assembler.Core
{
    public interface IHandler
    {
        void Handle(BaseFrame message);
    }
}