using System;
using System.Net.Http;
using Autofac;
using Common.Log;

namespace Lykke.Service.ClientAccountRecovery.Client
{
    public static class AutofacExtension
    {
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

        public static void RegisterClientAccountRecoveryClient(this ContainerBuilder builder, ClientAccountRecoveryServiceClientSettings settings, ILog log)
        {
            builder.RegisterClientAccountRecoveryClient(settings?.ServiceUrl, log);
        }
    }
}
