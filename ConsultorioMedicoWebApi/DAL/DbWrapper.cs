using System;
using System.Configuration;

namespace ConsultorioMedicoWebApi.DAL
{
    public partial class DbWrapper : BaseDbWrapper
    {
        protected override string SQLConnectionString { get; }
        protected override TimeSpan SQLCommandTimeOut { get; }

        public DbWrapper()
        {
            // Prioridad: variable de entorno sConSql (producción), si no el connectionString cCon.
            var env = Environment.GetEnvironmentVariable("sConSql");
            var cfg = ConfigurationManager.ConnectionStrings["cCon"]?.ConnectionString;
            SQLConnectionString = !string.IsNullOrWhiteSpace(env) ? env : cfg;

            if (string.IsNullOrWhiteSpace(SQLConnectionString))
                throw new InvalidOperationException("Cadena de conexión no configurada (sConSql o cCon).");

            SQLCommandTimeOut = TimeSpan.FromSeconds(15);
        }
    }
}
