﻿using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;

namespace HttpGatewayWebApi.swagger
{
    public class AssignOperationVendorExtensions : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            operation.Extensions.Add("x-purpose", "test");
        }
    }
}
