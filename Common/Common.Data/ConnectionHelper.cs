using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DESI.ConsultorioMedico.Common.Data;

/// <summary>
/// Construye conexiones <see cref="SqlConnection"/> a partir de la configuración.
/// </summary>
public static class ConnectionHelper
{
    /// <summary>
    /// Crea una conexión usando la cadena <c>ConnectionStrings:DefaultConnection</c>.
    /// </summary>
    public static SqlConnection CreateConnection(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");

        return new SqlConnection(connectionString);
    }
}
