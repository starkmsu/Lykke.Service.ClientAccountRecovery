using Lykke.Service.ClientAccountRecovery.Validation;
using Microsoft.AspNetCore.Http;

namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class UploadSelfieRequest
    {
        /// <summary>
        ///     JWE token containing current state of recovery process.
        /// </summary>
        [JweToken]
        public string StateToken { get; set; }

        /// <summary>
        ///     File to upload as selfie.
        /// </summary>
        public IFormFile File { get; set; }
    }
}
