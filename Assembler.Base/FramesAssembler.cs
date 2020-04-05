using System;
using System.Collections.Generic;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Microsoft.Extensions.Logging;

namespace Assembler.Base
{
    public class FramesAssembler<TFrame> : IAssembler<TFrame>
        where TFrame : BaseFrame
    {
        private readonly IResolver<AssemblingPosition, IFrameHandler<TFrame>> _typeToHandlerResolver;
        private readonly ILogger _logger;

        public FramesAssembler(IResolver<AssemblingPosition, IFrameHandler<TFrame>> typeToHandlerResolver,
            ILoggerFactory loggerFactory)
        {
            _typeToHandlerResolver = typeToHandlerResolver;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public void Assemble(TFrame frame)
        {
            IFrameHandler<TFrame> handler = ResolveHandler(frame);

            _logger.LogDebug($"Frame [{frame.Guid}] is being sent to the handler.");

            HandleFrame(frame, handler);
        }

        private IFrameHandler<TFrame> ResolveHandler(TFrame frame)
        {
            try
            {
                return _typeToHandlerResolver.Resolve(frame.AssemblingPosition);
            }
            catch (Exception e)
            {
                _logger.LogWarning($"No matching handler was found for the message [{frame.Guid}]," +
                                   $"It won't be used in the assembling process. \n {e}");
                return null;
            }
        }

        private void HandleFrame(TFrame frame, IFrameHandler<TFrame> handler)
        {
            try
            {
                handler?.Handle(frame);
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Failed handling the message [{frame.Guid}]," +
                                   $"It won't be used in the assembling process. \n {e}");
            }
        }
    }
}