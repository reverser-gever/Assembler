using System.Collections.Generic;
using Assembler.Core;

namespace Assembler.Base
{
    public class GeneralResolver<TIn, TOut> : IResolver<TIn, TOut>
    {
        // Copy from the one we have at Team.Util

        private readonly IDictionary<TIn, TOut> _mapping;

        public GeneralResolver(IDictionary<TIn, TOut> mapping)
        {
            _mapping = mapping;
        }

        public TOut Resolve(TIn input)
        {
            if (_mapping.ContainsKey(input))
            {
                return _mapping[input];
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }
    }
}