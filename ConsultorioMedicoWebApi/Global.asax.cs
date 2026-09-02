using System;
using System.Web.Http;
using Serilog;

namespace ConsultorioMedicoWebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(
                    path: System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/logs/log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 31)
                .CreateLogger();

            GlobalConfiguration.Configure(App_Start.WebApiConfig.Register);
        }

        protected void Application_End(object sender, EventArgs e)
        {
            Log.CloseAndFlush();
        }
    }
}
