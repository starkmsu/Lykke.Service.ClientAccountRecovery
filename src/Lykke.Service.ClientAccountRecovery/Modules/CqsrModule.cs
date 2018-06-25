using System;
using System.Collections.Generic;
using Autofac;
using Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Messaging;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.Session.Modules
{
    public class CqrsModule : Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            MessagePackSerializerFactory.Defaults.FormatterResolver = MessagePack.Resolvers.ContractlessStandardResolver.Instance;

            builder.RegisterType<AutofacDependencyResolver>().As<IDependencyResolver>().SingleInstance();


            builder.Register(ctx =>
                {

                    var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory { Uri = ctx.Resolve<IReloadingManager<AppSettings>>().Nested(n => n.ClientAccountRecoveryService.RabbitMq.ConnectionString).CurrentValue };

                    var messagingEngine = new MessagingEngine(ctx.Resolve<ILog>(),
                        new TransportResolver(new Dictionary<string, TransportInfo>
                        {
                            {"RabbitMq", new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName, rabbitMqSettings.Password, "None", "RabbitMq")}
                        }),
                        new RabbitMqTransportFactory());

                    return new CqrsEngine(ctx.Resolve<ILog>(),
                        ctx.Resolve<IDependencyResolver>(),
                        messagingEngine,
                        new DefaultEndpointProvider(),
                        true,
                        Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver(
                            "RabbitMq",
                            "messagepack",
                            environment: "lykke",
                            exclusiveQueuePostfix: "k8s")),

                        Register.BoundedContext(Consts.BoundedContext)
                            .PublishingEvents(typeof(SelfiePostedEvent)).With("events")
                    );
                })
                .As<ICqrsEngine>()
                .SingleInstance();
        }
    }

    internal class AutofacDependencyResolver : IDependencyResolver
    {
        private readonly ILifetimeScope _context;

        public AutofacDependencyResolver(ILifetimeScope kernel)
        {
            _context = kernel ?? throw new ArgumentNullException(nameof(kernel));
        }

        public object GetService(Type type)
        {
            return _context.Resolve(type);
        }

        public bool HasService(Type type)
        {
            return _context.IsRegistered(type);
        }
    }
}
