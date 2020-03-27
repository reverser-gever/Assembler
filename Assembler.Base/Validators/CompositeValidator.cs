using System;
using System.Collections.Generic;
using System.Linq;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base.Validators
{
    public class CompositeValidator<TMessageInAssembly> : IValidator<TMessageInAssembly>
        where TMessageInAssembly : BaseMessageInAssembly
    {
        private readonly IEnumerable<IValidator<TMessageInAssembly>> _validators;
        private readonly Operator _operator;

        public CompositeValidator(IEnumerable<IValidator<TMessageInAssembly>> validators, Operator @operator)
        {
            _validators = validators;
            _operator = @operator;
        }

        public bool IsValid(TMessageInAssembly messageInAssembly)
        {
            var validatorsResults = _validators.Select(validator => validator.IsValid(messageInAssembly)).ToList();

            switch (_operator)
            {
                case Operator.And:
                    return validatorsResults.TrueForAll(result => result.Equals(true));

                case Operator.Or:
                    return validatorsResults.Any(result => result.Equals(true));

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}