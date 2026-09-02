using System;
using System.Web.Mvc;
using System.Web.Routing;
using ConsultorioMedicoMVC.App_Start;
using Serilog;

namespace ConsultorioMedicoMVC
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(
                    path: System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/logs/log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 31)
                .CreateLogger();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            ModelBinders.Binders.Add(typeof(DateTime), new DateTimeModelBinder());
            ModelBinders.Binders.Add(typeof(DateTime?), new DateTimeModelBinder());
        }

        protected void Application_End(object sender, EventArgs e)
        {
            Log.CloseAndFlush();
        }
    }
}
