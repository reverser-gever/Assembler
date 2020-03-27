﻿using System;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base.FrameHandlers
{
    public class FinalFrameHandler<TFrame, TMessageInAssembly> : BaseFrameHandler<TFrame, TMessageInAssembly>
        where TFrame : BaseFrame
        where TMessageInAssembly : BaseMessageInAssembly
    {
        private readonly ILogger _logger;

        public FinalFrameHandler(ITimeBasedCache<TMessageInAssembly> timeBasedCache,
            IFactory<TFrame, string> identifierFactory, ICreator<TMessageInAssembly> messageInAssemblyCreator,
            IMessageEnricher<TFrame, TMessageInAssembly> messageInAssemblyEnricher,
            IMessageReleaser<TMessageInAssembly> messageReleaser, ILoggerFactory loggerFactory)
            : base(timeBasedCache, identifierFactory, messageInAssemblyEnricher, messageInAssemblyCreator,
                messageReleaser)
        {
            _logger = loggerFactory.GetLogger(this);
        }

        public override void Handle(TFrame frame)
        {
            string identifier;

            try
            {
                identifier = GetIdentifier(frame);
            }
            catch (Exception e)
            {
                _logger.Error($"There was an error while getting an identifier out of the frame [{frame.Guid}], " +
                              $"it won't be used it the assembling process \n {e}");
                return;
            }

            if (TryGetOrCreateMessageInAssembly(identifier, out var message))
            {
                EnrichMessage(frame, message);
                _logger.Debug(
                    $"Enriched [{message.Guid}] with the frame [{frame.Guid}].");

                ReleaseMessage(message, ReleaseReason.EndReceived);
            }
            else
            {
                _logger.Debug(
                    $"An end message [{frame.Guid}] was received, " +
                    "but there where no open message in assembly with the same identifier. " +
                    "It will not be used.");
            }
        }

        private bool TryGetOrCreateMessageInAssembly(string identifier, out TMessageInAssembly message)
        {
            if (TimeBasedCache.Exists(identifier))
            {
                message = TimeBasedCache.Get<TMessageInAssembly>(identifier);
                TimeBasedCache.Remove(identifier);

                _logger.Debug(
                    $"The message [{message.Guid}] was removed from the cache, it's set for release.");
            }
            else
            {
                message = CreateMessage();

                _logger.Debug($"A new message was created [{message.Guid}].");
            }

            return true;
        }
    }
}