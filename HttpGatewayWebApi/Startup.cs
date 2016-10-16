using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using Microsoft.Extensions.PlatformAbstractions;
using System.IO;
using HttpGatewayWebApi.swagger;
using Iridium.GatewayCore;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.ServiceFabric.Services.Client;
using System;
using System.Linq;

namespace HttpGatewayWebApi
{
    /// <summary>
    /// The startup.
    /// </summary>
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnv;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">
        /// The env.
        /// </param>
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            _hostingEnv = env;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // Inject an implementation of ISwaggerProvider with defaulted settings applied
            IConfigurationSection swaggerConfig = Configuration.GetSection("Swagger");
            
            services.AddSwaggerGen(c =>
            {
                c.SingleApiVersion(new Info
                {
                    Version = $"v{swaggerConfig["Version"]}",
                    Title = swaggerConfig["Title"],
                    Description = swaggerConfig["Description"],
                    TermsOfService = swaggerConfig["TermsOfService"],
                    Contact = new Contact()
                    {
                        Email = swaggerConfig["ContactEmail"],
                        Name = swaggerConfig["ContactName"],
                        Url = swaggerConfig["ContactUrl"],
                    },
                });

                c.OperationFilter<AssignOperationVendorExtensions>();
            });

            //if (_hostingEnv.IsDevelopment())
            //{
                services.ConfigureSwaggerGen(c =>
                {
                    c.IncludeXmlComments(GetXmlCommentsPath(PlatformServices.Default.Application));
                });
            //}

            services.AddDefaultHttpRequestDispatcherProvider();
        }
        
        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">
        /// The application.
        /// </param>
        /// <param name="env">
        /// The environment.
        /// </param>
        /// <param name="loggerFactory">
        /// The logger factory.
        /// </param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Configure the HTTP request pipeline.
            app.UseStaticFiles();

            //if (_hostingEnv.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}

            app.UseMvc();

            // Load Swagger configuration
            IConfigurationSection swaggerConfig = Configuration.GetSection("Swagger");

            // Enable middleware to serve generated Swagger as a JSON endpoint            
            app.UseSwagger((httpRequest, swaggerDoc) =>
            {
                swaggerDoc.Host = httpRequest.Host.Value;
            });

            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)            
            app.UseSwaggerUi("swagger-ui", $"/swagger/v{swaggerConfig["Version"]}/swagger.json");

            // Enable Gateway for all routes
            app.MapGateway("/api/v1/values", "HttpGatewayApplication", "HttpGatewayWebApi");

            //Infinit Loop :(
            //app.MapGateway("", "HttpGatewayApplication", "HttpGatewayWebApi");

#pragma warning disable 1998
            app.Run(
                async request =>
                {
                    request.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await
                        request.Response.WriteAsync(
                            "Unrecognized request. The Gateway is not configured to handle it");
                });
#pragma warning restore 1998

        }

        private string GetXmlCommentsPath(ApplicationEnvironment appEnvironment)
        {
            return Path.Combine(appEnvironment.ApplicationBasePath, "HttpGatewayWebApi.xml");
        }

        private ServicePartitionKey GetPartitionAsync(HttpContext context)
        {
            var pathSegments = context.Request.Path.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var updateId = pathSegments.First();
            var hashCode = Fnv1AHashCode.Get64BitHashCode(updateId);
            return new ServicePartitionKey(hashCode);
        }
    }
}
