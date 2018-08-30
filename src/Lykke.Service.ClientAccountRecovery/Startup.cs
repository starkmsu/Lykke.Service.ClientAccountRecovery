using System;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.ClientAccountRecovery.Middleware;
using Lykke.Service.ClientAccountRecovery.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;

namespace Lykke.Service.ClientAccountRecovery
{
    [UsedImplicitly]
    internal class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "ClientAccountRecovery API",
            ApiVersion = "v1"
        };

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.SwaggerOptions = _swaggerOptions;

                options.Logs = logs =>
                {
                    logs.AzureTableName = "ClientAccountRecoveryLog";
                    logs.AzureTableConnectionStringResolver =
                        settings => settings.ClientAccountRecoveryService.Db.LogsConnString;
                };

                // Extend the service configuration
                options.Extend = (sc, settings) =>
                {
                    sc.Configure<MvcJsonOptions>(c =>
                    {
                        // Serialize all properties to camelCase by default
                        c.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    });

                    ApiKeyAuthAttribute.ApiKey = settings.CurrentValue.ClientAccountRecoveryService.ApiKey;
                };


                // Extended Swagger configuration
                options.Swagger = swagger =>
                {
                    swagger.OperationFilter<AddRequiredHeaderParameter>();
                    swagger.AddSecurityDefinition("CustomScheme", new ApiKeyScheme
                    {
                        In = "header",
                        Description = "Please insert API key into field",
                        Name = ApiKeyAuthAttribute.HeaderName,
                        Type = "apiKey"
                    });
                };
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app)
        {
            app.UseLykkeConfiguration(options => { options.SwaggerOptions = _swaggerOptions; });
        }
    }
}
