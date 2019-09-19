using System.Security.Cryptography;
using System.Web.Http;
using SecuroteckWebApplication.Controllers;

namespace SecuroteckWebApplication
{
    public static class WebApiConfig
    {
        // Publically accessible global static variables could go here
        public static RSACryptoServiceProvider RSAProvider;
        public static string PublicKey;
        public static string PrivateKey;
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            GlobalConfiguration.Configuration.MessageHandlers.Add(new APIAuthorisationHandler());

            #region Task 11
            // Configuration for Task 11
            RSAProvider = new RSACryptoServiceProvider();
            PublicKey = RSAProvider.ToXmlString(false);
            PrivateKey = RSAProvider.ToXmlString(true);
            #endregion

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "TalkbackApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
