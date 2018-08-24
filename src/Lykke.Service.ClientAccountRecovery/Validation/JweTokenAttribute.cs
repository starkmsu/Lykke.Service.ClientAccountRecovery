using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Lykke.Common.Validation.JweToken;
using Lykke.Service.ClientAccountRecovery.Strings;

namespace Lykke.Service.ClientAccountRecovery.Validation
{
    internal class JweTokenAttribute : ValidationAttribute
    {
        private static readonly JweTokenValidator JweTokenValidator = new JweTokenValidator();

        private static readonly IDictionary<JweTokenErrorCode, string> JweTokenErrorMapping =
            new Dictionary<JweTokenErrorCode, string>
            {
                {JweTokenErrorCode.NullOrEmpty, Phrases.RequiredField},
                {JweTokenErrorCode.NotJweToken, Phrases.InvalidJweToken}
            };

        private static readonly string DefaultErrorMessage = Phrases.InvalidJweToken;


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var result = JweTokenValidator.Validate(value as string);

            if (result.IsValid)
                return ValidationResult.Success;

            var errorCode = result.ErrorCodes.FirstOrDefault();

            var message = JweTokenErrorMapping.ContainsKey(errorCode)
                ? JweTokenErrorMapping[errorCode]
                : DefaultErrorMessage;

            return new ValidationResult(string.Format(message, validationContext.DisplayName));
        }
    }
}
