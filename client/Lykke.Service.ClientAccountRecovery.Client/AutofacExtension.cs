using System;
using Autofac;
using JetBrains.Annotations;
using Lykke.HttpClientGenerator;
using Lykke.HttpClientGenerator.Infrastructure;

namespace Lykke.Service.ClientAccountRecovery.Client
{
    [PublicAPI]
    public static class AutofacExtension
    {
        /// <summary>
        ///     Registers <see cref="IClientAccountRecoveryServiceClient" /> in Autofac container using
        ///     <see cref="ClientAccountRecoveryServiceClientSettings" />.
        /// </summary>
        /// <param name="builder">Autofac container builder.</param>
        /// <param name="settings">ClientAccountRecoveryService client settings.</param>
        /// <param name="builderConfigure">Optional <see cref="HttpClientGeneratorBuilder" /> configure handler.</param>
        public static void RegisterClientAccountRecoveryServiceClient(
            [NotNull] this ContainerBuilder builder,
            [NotNull] ClientAccountRecoveryServiceClientSettings settings,
            [CanBeNull] Func<HttpClientGeneratorBuilder, HttpClientGeneratorBuilder> builderConfigure = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(settings.ServiceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.",
                    nameof(ClientAccountRecoveryServiceClientSettings.ServiceUrl));
            if (string.IsNullOrWhiteSpace(settings.ApiKey))
                throw new ArgumentException("Value cannot be null or whitespace.",
                    nameof(ClientAccountRecoveryServiceClientSettings.ApiKey));

            var clientBuilder = HttpClientGenerator.HttpClientGenerator.BuildForUrl(settings.ServiceUrl)
                .WithApiKey(settings.ApiKey)
                .WithoutCaching()
                .WithoutRetries()
                .WithAdditionalCallsWrapper(new ExceptionHandlerCallsWrapper());

            clientBuilder = builderConfigure?.Invoke(clientBuilder) ?? clientBuilder;

            builder.RegisterInstance(new ClientAccountRecoveryServiceClient(clientBuilder.Create()))
                .As<IClientAccountRecoveryServiceClient>()
                .SingleInstance();
        }
    }
}
