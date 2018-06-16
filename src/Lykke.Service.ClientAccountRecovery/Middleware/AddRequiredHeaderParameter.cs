using System.Collections.Generic;
using System.Linq;
using Lykke.Service.ClientAccountRecovery.Middleware;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.ClientAccountRecovery
{
    internal class AddRequiredHeaderParameter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var hasHeader = context.ApiDescription.ActionAttributes()
                .OfType<ApiKeyAuthAttribute>()
                .Any();

            if (!hasHeader)
            {
                return;
            }

            if (operation.Parameters == null)
            {
                operation.Parameters = new List<IParameter>();
            }
            operation.Parameters.Add(new NonBodyParameter
            {
                Required = true,
                In = "header",
                Type = "string",
                Name = ApiKeyAuthAttribute.HeaderName,
                Description = "An API key."
            });
        }
    }
}
