using System;
using Assembler.Core;

namespace Assembler.Base
{
    public class DateTimeProvider : IDateTimeProvider
    {
        // Replace with Team.Util
        public DateTime Now => DateTime.Now;
    }
}