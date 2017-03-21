// -----------------------------------------------------------------------
// <copyright file="WebApiConfig.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Bot
{
    using System.Web.Http;
    using System.Web.Http.ExceptionHandling;
    using Autofac.Integration.WebApi;
    using Logic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Provides the configurations for the Web API. 
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Registers the dependency resolver and routes.
        /// </summary>
        /// <param name="config">An instance of <see cref="HttpConfiguration"/> to be configured.</param>
        public static void Register(HttpConfiguration config)
        {
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            };

            config.Services.Add(typeof(IExceptionLogger), new AiExceptionLogger());
            config.DependencyResolver = new AutofacWebApiDependencyResolver(WebApiApplication.Container);

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });
        }
    }
}