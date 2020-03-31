using System;
using System.Collections.Generic;
using System.Linq;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base.Releasing
{
    public class CompositeValidator<TMessageInAssembly> : IValidator<TMessageInAssembly>
        where TMessageInAssembly : BaseMessageInAssembly
    {
        private readonly IEnumerable<IValidator<TMessageInAssembly>> _validators;
        private readonly LogicalOperator _logicalOperator;

        public CompositeValidator(IEnumerable<IValidator<TMessageInAssembly>> validators, LogicalOperator logicalOperator)
        {
            _validators = validators;
            _logicalOperator = logicalOperator;
        }

        public bool IsValid(TMessageInAssembly messageInAssembly)
        {
            bool IsValid(IValidator<TMessageInAssembly> validator) => validator.IsValid(messageInAssembly);

            switch (_logicalOperator)
            {
                case LogicalOperator.And:
                    return _validators.All(IsValid);

                case LogicalOperator.Or:
                    return _validators.Any(IsValid);

                default:
                    throw new NotSupportedException($"Logical LogicalOperator [{_logicalOperator}] is not supported.");
            }
        }
    }
}