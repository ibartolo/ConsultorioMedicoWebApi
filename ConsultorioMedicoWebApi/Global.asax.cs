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
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.File(
                    path: System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/logs/log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 31,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

            GlobalConfiguration.Configure(App_Start.WebApiConfig.Register);
        }

        protected void Application_End(object sender, EventArgs e)
        {
            Log.CloseAndFlush();
        }
    }
}
