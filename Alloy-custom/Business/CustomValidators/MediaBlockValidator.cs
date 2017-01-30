using System.Collections.Generic;
using System.Linq;
using Alloy_custom.Models.Blocks;
using EPiServer.Core;
using EPiServer.Validation;

namespace Alloy_custom.Business.CustomValidators
{
    public class MediaBlockValidator : IValidate<MediaBlock>
    {
        public IEnumerable<ValidationError> Validate(MediaBlock currentBlock)
        {
            var url = currentBlock.YouTubeLink;
            if (url == string.Empty)
            {
                return new[]
                {
                    new ValidationError()
                    {
                        ErrorMessage = "Not a valid youtube url",
                        PropertyName = currentBlock.GetPropertyName(b => b.YouTubeLink),
                        Severity = ValidationErrorSeverity.Error,
                        ValidationType = ValidationErrorType.AttributeMatched
                    }
                };

            }
            return Enumerable.Empty<ValidationError>();
        }
    }
}