using System;
using System.Collections.Generic;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

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
            _logger = loggerFactory.GetLogger(this);
        }

        public void Assemble(TFrame frame)
        {
            try
            {
                IFrameHandler<TFrame> handler = _typeToHandlerResolver.Resolve(frame.AssemblingPosition);

                _logger.Debug($"Frame [{frame.Guid}] is being sent to the handler.");

                handler.Handle(frame);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error($"No matching resolver was found for the message [{frame.Guid}]," +
                              $"It won't be used in the assembling process. \n {e}");
            }
        }
    }
}