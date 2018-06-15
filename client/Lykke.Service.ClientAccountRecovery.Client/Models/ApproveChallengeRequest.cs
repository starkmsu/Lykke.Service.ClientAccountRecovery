// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.Service.ClientAccountRecovery.Client.Models
{
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using System.Linq;

    public partial class ApproveChallengeRequest
    {
        /// <summary>
        /// Initializes a new instance of the ApproveChallengeRequest class.
        /// </summary>
        public ApproveChallengeRequest()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ApproveChallengeRequest class.
        /// </summary>
        /// <param name="challenge">Possible values include: 'Unknown', 'Sms',
        /// 'Email', 'Selfie', 'Words', 'Device', 'Pin', 'Undefined'</param>
        /// <param name="checkResult">Possible values include: 'Unknown',
        /// 'Approved', 'Rejected'</param>
        public ApproveChallengeRequest(Challenge challenge, CheckResult checkResult, string recoveryId = default(string), string agentId = default(string))
        {
            RecoveryId = recoveryId;
            Challenge = challenge;
            AgentId = agentId;
            CheckResult = checkResult;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "recoveryId")]
        public string RecoveryId { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'Unknown', 'Sms', 'Email',
        /// 'Selfie', 'Words', 'Device', 'Pin', 'Undefined'
        /// </summary>
        [JsonProperty(PropertyName = "challenge")]
        public Challenge Challenge { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "agentId")]
        public string AgentId { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'Unknown', 'Approved',
        /// 'Rejected'
        /// </summary>
        [JsonProperty(PropertyName = "checkResult")]
        public CheckResult CheckResult { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (RecoveryId != null)
            {
                if (RecoveryId.Length < 8)
                {
                    throw new ValidationException(ValidationRules.MinLength, "RecoveryId", 8);
                }
            }
        }
    }
}
