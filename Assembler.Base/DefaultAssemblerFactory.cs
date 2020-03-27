using System.Collections.Generic;
using Assembler.Base.Creators;
using Assembler.Base.FrameHandlers;
using Assembler.Base.MessageEnrichers;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Assembler.Core.RawAssemblingEntities;

namespace Assembler.Base
{
    public class DefaultAssemblerFactory<TFrame, TMessageInAssembly>
        where TFrame : BaseFrame
        where TMessageInAssembly : BaseMessageInAssembly
    {
        public IAssembler<TFrame> Create(
            ITimeBasedCache<TMessageInAssembly> timeBasedCache,
            IFactory<TFrame, string> identifierFactory, ICreator<TMessageInAssembly> messageInAssemblyCreator,
            IMessageEnricher<TFrame, TMessageInAssembly> initialFrameMessageEnricher,
            IMessageEnricher<TFrame, TMessageInAssembly> middleFrameMessageEnricher,
            IMessageEnricher<TFrame, TMessageInAssembly> finalFrameMessageEnricher,
            IMessageReleaser<TMessageInAssembly> messageReleaser, ILoggerFactory loggerFactory)
        {
            var initialFrameHandler = new InitialFrameHandler<TFrame, TMessageInAssembly>(timeBasedCache,
                identifierFactory, messageInAssemblyCreator, initialFrameMessageEnricher, messageReleaser, loggerFactory);
            var middleFrameHandler = new MiddleFrameHandler<TFrame, TMessageInAssembly>(timeBasedCache,
                identifierFactory, messageInAssemblyCreator, middleFrameMessageEnricher, messageReleaser, loggerFactory);
            var finalFrameHandler = new FinalFrameHandler<TFrame, TMessageInAssembly>(timeBasedCache,
                identifierFactory, messageInAssemblyCreator, finalFrameMessageEnricher, messageReleaser, loggerFactory);

            var resolver = new GeneralResolver<AssemblingPosition, IFrameHandler<TFrame>>(
                new Dictionary<AssemblingPosition, IFrameHandler<TFrame>>
            {
                {AssemblingPosition.Initial, initialFrameHandler },
                {AssemblingPosition.Middle, middleFrameHandler },
                {AssemblingPosition.Final, finalFrameHandler }
            });

            return new FramesAssembler<TFrame>(resolver, loggerFactory);
        }

        public IAssembler<TFrame> CreateRawAssembler(
            ITimeBasedCache<RawMessageInAssembly> timeBasedCache, IFactory<TFrame, string> identifierFactory,
            ILoggerFactory loggerFactory, IMessageReleaser<RawMessageInAssembly> messageReleaser = null)
        {
            if (messageReleaser == null)
            {
                messageReleaser = new MessageReleaser<RawMessageInAssembly>(timeBasedCache, loggerFactory);
            }

            var messageInAssemblyCreator = new RawMessageInAssemblyCreator();
            var rawInitialFrameMessageEnricher = new RawInitialFrameMessageEnricher();
            var rawMiddleFrameMessageEnricher = new RawMiddleFrameMessageEnricher();
            var rawFinalFrameMessageEnricher = new RawFinalFrameMessageEnricher();

            var initialFrameHandler = new InitialFrameHandler<TFrame, RawMessageInAssembly>(timeBasedCache,
                identifierFactory, messageInAssemblyCreator, rawInitialFrameMessageEnricher, messageReleaser,
                loggerFactory);
            var middleFrameHandler = new MiddleFrameHandler<TFrame, RawMessageInAssembly>(timeBasedCache,
                identifierFactory, messageInAssemblyCreator, rawMiddleFrameMessageEnricher, messageReleaser,
                loggerFactory);
            var finalFrameHandler = new FinalFrameHandler<TFrame, RawMessageInAssembly>(timeBasedCache,
                identifierFactory, messageInAssemblyCreator, rawFinalFrameMessageEnricher, messageReleaser,
                loggerFactory);

            var resolver = new GeneralResolver<AssemblingPosition, IFrameHandler<TFrame>>(
                new Dictionary<AssemblingPosition, IFrameHandler<TFrame>>
                {
                    {AssemblingPosition.Initial, initialFrameHandler},
                    {AssemblingPosition.Middle, middleFrameHandler},
                    {AssemblingPosition.Final, finalFrameHandler}
                });

            return new FramesAssembler<TFrame>(resolver, loggerFactory);
        }
    }
}