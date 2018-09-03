using System;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lykke.Service.ClientAccountRecovery.Validation
{
    /// <summary>
    ///     This attribute allows to enlarge request max size.
    ///     To setup selfie max size please change setting <see cref="IRecoveryFileSettings" />.
    /// </summary>
    public class SelfieMaxSizeAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => true;

        public IFilterMetadata CreateInstance(
            IServiceProvider serviceProvider)
        {
            if (!(serviceProvider.GetService(typeof(IRecoveryFileSettings)) is IRecoveryFileSettings
                recoveryFileSettings))
                throw new Exception($"{nameof(IRecoveryFileSettings)} is not registered in dependency container!");

            var maxSizeMb = recoveryFileSettings.SelfieImageMaxSizeMBytes;

            var attribute = new RequestSizeLimitAttribute(maxSizeMb * 1024 * 1024);

            return attribute.CreateInstance(serviceProvider);
        }
    }
}
