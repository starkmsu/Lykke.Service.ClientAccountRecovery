namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class SubmitChallengeResponse
    {
        /// <summary>
        ///     JWE token containing current state of recovery process.
        /// </summary>
        public string StateToken { get; set; }

        /// <summary>
        ///     Status of recovery operation.
        /// </summary>
        public OperationStatus OperationStatus { get; set; }

        /// <summary>
        /// Create error response.
        /// </summary>
        /// <param name="message">Error message to be included in response.</param>
        /// <returns>Response with error message.</returns>
        public static SubmitChallengeResponse CreateError(string message)
        {
            return new SubmitChallengeResponse
            {
                OperationStatus = new OperationStatus
                {
                    Error = true,
                    Message = message
                },
                StateToken = null
            };
        }

        /// <summary>
        /// Create successful response.
        /// </summary>
        /// <param name="token">State Token to be included in response.</param>
        /// <returns>Response with state token.</returns>
        public static SubmitChallengeResponse CreateSuccess(string token)
        {
            return new SubmitChallengeResponse
            {
                OperationStatus = new OperationStatus
                {
                    Error = false,
                    Message = null
                },
                StateToken = token
            };
        }
    }
}
