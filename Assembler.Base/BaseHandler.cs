using Assembler.Core;
using Assembler.Core.Entities;

namespace Assembler.Base
{
    public abstract class BaseHandler<TFrame, TMessage> : IHandler
        where TFrame : BaseFrame
        where TMessage : BaseMessageInAssembly
    {
        protected readonly ITimeBasedCache<TMessage> Cache;
        protected readonly IFactory<TFrame, string> IdentifierFactory;

        protected BaseHandler(ITimeBasedCache<TMessage> cache, IFactory<TFrame, string> identifierFactory)
        {
            Cache = cache;
            IdentifierFactory = identifierFactory;
        }

        protected string GetIdentifier(TFrame frame) => IdentifierFactory.Create(frame);

        public abstract void Handle(BaseFrame message);

        protected abstract void EnrichMessageWithFrame(TFrame frame, TMessage message);
    }
}