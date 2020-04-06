using System;

namespace Assembler.Core
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }
}