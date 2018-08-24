using System.Threading.Tasks;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.Core.Services
{
    /// <summary>
    ///     Service for working with recovery token.
    /// </summary>
    public interface IRecoveryTokenService
    {
        /// <summary>
        ///     Get token payload and cast it to <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">Type to deserialize payload to.</typeparam>
        /// <param name="stateToken">Recovery token.</param>
        /// <returns>Deserialized payload object of type <typeparamref name="T" />.</returns>
        Task<T> GetTokenPayloadAsync<T>(string stateToken);

        /// <summary>
        ///     Get recovery id from recovery token.
        /// </summary>
        /// <param name="stateToken">Recovery token.</param>
        /// <returns>Recovery id.</returns>
        Task<string> GetRecoveryIdAsync(string stateToken);

        /// <summary>
        ///     Generates encrypted JWE recovery token from recovery context <paramref name="context" />.
        /// </summary>
        /// <param name="context">Recovery context.</param>
        /// <returns>Recovery token.</returns>
        Task<string> GenerateTokenAsync(RecoveryContext context);
    }
}
