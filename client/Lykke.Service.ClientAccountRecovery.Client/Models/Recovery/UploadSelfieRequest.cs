using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Lykke.Service.ClientAccountRecovery.Client.Models.Recovery
{
    [PublicAPI]
    public class UploadSelfieRequest
    {
        /// <summary>
        ///     JWE token containing current state of recovery process.
        /// </summary>
        public string StateToken { get; set; }

        /// <summary>
        ///     File to upload as selfie.
        /// </summary>
        public IFormFile File { get; set; }
    }
}
