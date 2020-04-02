using System.Collections.Generic;
using Assembler.Base.Creators;
using Assembler.Base.FrameHandlers;
using Assembler.Base.MessageEnrichers;
using Assembler.Base.Releasing;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Assembler.Core.RawAssemblingEntities;
using Assembler.Core.Releasing;
using Microsoft.Extensions.Logging;

namespace Assembler.Base
{
    public class DefaultAssemblerFactory<TFrame, TMessageInAssembly>
        where TFrame : BaseFrame
        where TMessageInAssembly : BaseMessageInAssembly
    {
        // TODO: Make it a builder after the CR
        public IAssembler<TFrame> Create(
            ITimeBasedCache<TMessageInAssembly> timeBasedCache,
            IIdentifierGenerator<TFrame> identifierGenerator,
            IMessageInAssemblyCreator<TMessageInAssembly> messageInAssemblyCreator,
            IMessageEnricher<TFrame, TMessageInAssembly> initialFrameMessageEnricher,
            IMessageEnricher<TFrame, TMessageInAssembly> middleFrameMessageEnricher,
            IMessageEnricher<TFrame, TMessageInAssembly> finalFrameMessageEnricher,
            IMessageInAssemblyReleaser<TMessageInAssembly> messageInAssemblyReleaser, ILoggerFactory loggerFactory)
        {
            var dateTimeProvider = new DateTimeProvider();

            var initialFrameHandler = new InitialFrameHandler<TFrame, TMessageInAssembly>(timeBasedCache,
                identifierGenerator, messageInAssemblyCreator, initialFrameMessageEnricher, messageInAssemblyReleaser,
                dateTimeProvider, loggerFactory);
            var middleFrameHandler = new MiddleFrameHandler<TFrame, TMessageInAssembly>(timeBasedCache,
                identifierGenerator, messageInAssemblyCreator, middleFrameMessageEnricher, messageInAssemblyReleaser,
                dateTimeProvider, loggerFactory);
            var finalFrameHandler = new FinalFrameHandler<TFrame, TMessageInAssembly>(timeBasedCache,
                identifierGenerator, messageInAssemblyCreator, finalFrameMessageEnricher, messageInAssemblyReleaser,
                dateTimeProvider, loggerFactory);

            var resolver = new GeneralResolver<AssemblingPosition, IFrameHandler<TFrame>>(
                new Dictionary<AssemblingPosition, IFrameHandler<TFrame>>
            {
                {AssemblingPosition.Initial, initialFrameHandler },
                {AssemblingPosition.Middle, middleFrameHandler },
                {AssemblingPosition.Final, finalFrameHandler }
            });

            messageInAssemblyReleaser.Start();

            return new FramesAssembler<TFrame>(resolver, loggerFactory);
        }

        public IAssembler<TFrame> CreateRawAssembler(ITimeBasedCache<RawMessageInAssembly> timeBasedCache,
            IIdentifierGenerator<TFrame> identifierGenerator, ILoggerFactory loggerFactory,
            IMessageInAssemblyReleaser<RawMessageInAssembly> messageInAssemblyReleaser = null)
        {
            var dateTimeProvider = new DateTimeProvider();

            if (messageInAssemblyReleaser == null)
            {
                messageInAssemblyReleaser = new MessageInAssemblyReleaser<RawMessageInAssembly>(timeBasedCache, loggerFactory);
            }

            var messageInAssemblyCreator = new RawMessageInAssemblyCreator(dateTimeProvider);
            var rawInitialFrameMessageEnricher = new RawInitialFrameMessageEnricher();
            var rawMiddleFrameMessageEnricher = new RawMiddleFrameMessageEnricher();
            var rawFinalFrameMessageEnricher = new RawFinalFrameMessageEnricher();

            var initialFrameHandler = new InitialFrameHandler<TFrame, RawMessageInAssembly>(timeBasedCache,
                identifierGenerator, messageInAssemblyCreator, rawInitialFrameMessageEnricher, messageInAssemblyReleaser,
                dateTimeProvider, loggerFactory);
            var middleFrameHandler = new MiddleFrameHandler<TFrame, RawMessageInAssembly>(timeBasedCache,
                identifierGenerator, messageInAssemblyCreator, rawMiddleFrameMessageEnricher, messageInAssemblyReleaser,
                dateTimeProvider, loggerFactory);
            var finalFrameHandler = new FinalFrameHandler<TFrame, RawMessageInAssembly>(timeBasedCache,
                identifierGenerator, messageInAssemblyCreator, rawFinalFrameMessageEnricher, messageInAssemblyReleaser,
                dateTimeProvider, loggerFactory);

            var resolver = new GeneralResolver<AssemblingPosition, IFrameHandler<TFrame>>(
                new Dictionary<AssemblingPosition, IFrameHandler<TFrame>>
                {
                    {AssemblingPosition.Initial, initialFrameHandler},
                    {AssemblingPosition.Middle, middleFrameHandler},
                    {AssemblingPosition.Final, finalFrameHandler}
                });

            messageInAssemblyReleaser.Start();

            return new FramesAssembler<TFrame>(resolver, loggerFactory);
        }
    }
}