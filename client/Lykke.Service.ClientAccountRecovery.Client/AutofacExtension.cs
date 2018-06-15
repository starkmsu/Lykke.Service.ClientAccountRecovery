using System;
using System.Net.Http;
using Autofac;
using Common.Log;

namespace Lykke.Service.ClientAccountRecovery.Client
{
    /// <summary>
    /// 
    /// </summary>
    public static class AutofacExtension
    {
        /// <summary>
        /// Registers a client implementation in the container
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="serviceUrl"></param>
        /// <param name="log"></param>
        public static void RegisterClientAccountRecoveryClient(this ContainerBuilder builder, string serviceUrl, ILog log)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (string.IsNullOrWhiteSpace(serviceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(serviceUrl));

            builder.Register(c => new ClientAccountRecoveryServiceClient(new Uri(serviceUrl), new HttpClient()))
                .As<IClientAccountRecoveryServiceClient>()
                .SingleInstance();
        }

        /// <summary>
        /// Registers a client implementation in the container
        /// </summary>
        public static void RegisterClientAccountRecoveryClient(this ContainerBuilder builder, ClientAccountRecoveryServiceClientSettings settings, ILog log)
        {
            builder.RegisterClientAccountRecoveryClient(settings?.ServiceUrl, log);
        }
    }
}
