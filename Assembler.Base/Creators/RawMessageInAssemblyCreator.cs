using Assembler.Core;
using Assembler.Core.RawAssemblingEntities;

namespace Assembler.Base.Creators
{
    public class RawMessageInAssemblyCreator : IMessageInAssemblyCreator<RawMessageInAssembly>
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public RawMessageInAssemblyCreator(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public RawMessageInAssembly Create()
        {
            var dateTimeNow = _dateTimeProvider.Now;
            return new RawMessageInAssembly(dateTimeNow, dateTimeNow);
        }
    }
}