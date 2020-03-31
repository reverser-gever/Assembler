using System;

namespace Assembler.Core
{
    public interface IStartable : IDisposable
    {
        void Start();
    }
}