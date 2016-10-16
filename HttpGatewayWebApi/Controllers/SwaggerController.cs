using HttpGatewayWebApi.swagger.model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Fabric;

namespace HttpGatewayWebApi.Controllers
{
    [Route("swagger-resources")]
    public class SwaggerController : Controller
    {
        // GET api/values
        [HttpGet]
        public IList<SwaggerResource> Get()
        {
            SwaggerResource resource = new SwaggerResource
            {
                Location = "/v2/api-docs",
                Name = "default",
                SwaggerVersion = "2.0"
            };

            SwaggerResource resource2 = new SwaggerResource
            {
                Location = "/gateway/v2/api-docs",
                Name = "gateway",
                SwaggerVersion = "2.0"
            };

            GetApplicationDeployed();

            return new List<SwaggerResource> { resource, resource2 };
        }

        private void GetApplicationDeployed() {

            // Create FabricClient with connection and security information here.
            FabricClient fabricClient = new FabricClient("Http:\\localhost:19080");
            // Retrieve all Application deployed on the cluster.
            var applications = fabricClient.QueryManager.GetApplicationListAsync().Result;
            // For each application, retrieve the list of Services attached to the applications.
            var services = fabricClient.QueryManager.GetServiceListAsync(new System.Uri($"fabric:/{applications[0].ApplicationName}"));
            
        }
    }
}
