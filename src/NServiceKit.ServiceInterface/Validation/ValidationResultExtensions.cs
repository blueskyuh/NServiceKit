using NServiceKit.FluentValidation.Results;
using NServiceKit.Validation;

namespace NServiceKit.ServiceInterface.Validation
{
    /// <summary>A validation result extensions.</summary>
    public static class ValidationResultExtensions
    {
        /// <summary>
        /// Converts the validation result to an error result which will be serialized by NServiceKit in a clean and human-readable way.
        /// </summary>
        /// <param name="result">The validation result</param>
        /// <returns></returns>
        public static ValidationErrorResult ToErrorResult(this ValidationResult result)
        {
            var validationResult = new ValidationErrorResult();
            foreach (var error in result.Errors)
                validationResult.Errors.Add(new ValidationErrorField(error.ErrorCode, error.PropertyName, error.ErrorMessage, error.AttemptedValue));

            return validationResult;
        }

        /// <summary>
        /// Converts the validation result to an error exception which will be serialized by NServiceKit in a clean and human-readable way
        /// if the returned exception is thrown.
        /// </summary>
        /// <param name="result">The validation result</param>
        /// <returns></returns>
        public static ValidationError ToException(this ValidationResult result)
        {
            return new ValidationError(result.ToErrorResult());
        }
    }
}

