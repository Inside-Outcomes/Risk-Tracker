using RiskTracker.Migrations;
using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace RiskTracker
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            DatabaseUpdate.Apply();
            DataConfig.InitialData();
      }
    }
}
