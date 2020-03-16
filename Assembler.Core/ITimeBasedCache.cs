using System;
using CacheManager.Core;

namespace Assembler.Core
{
    public interface ITimeBasedCache<TIn, out TOut> : ICache<TIn>
    {
        event Action<TOut> ItemExpired;
    }
}