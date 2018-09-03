using JetBrains.Annotations;

namespace Lykke.Service.ClientAccountRecovery.Client.Models.Recovery
{
    [PublicAPI]
    public class UploadSelfieResponse
    {
        /// <summary>
        ///     Id of uploaded image.
        /// </summary>
        public string FileId { get; set; }
    }
}
