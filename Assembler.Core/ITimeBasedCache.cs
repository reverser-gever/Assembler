using System;
using CacheManager.Core;

namespace Assembler.Core
{
    public interface ITimeBasedCache<TValue> : ICache<TValue>
    {
        //TODO: Replace with the one we have
        event Action<TValue> OnItemExpired;
    }
}