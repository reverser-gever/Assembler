using System;
using System.Collections.Generic;
using System.Linq;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;

namespace Assembler.Base.Validators
{
    public class ValidatorComposite<TMessageInAssembly> : IValidator<TMessageInAssembly>
        where TMessageInAssembly : BaseMessageInAssembly
    {
        private readonly IEnumerable<IValidator<TMessageInAssembly>> _validators;
        private readonly Operator _operator;

        public ValidatorComposite(IEnumerable<IValidator<TMessageInAssembly>> validators, Operator @operator)
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
                    return validatorsResults.All(result => true);

                case Operator.Or:
                    return validatorsResults.Any(result => true);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}