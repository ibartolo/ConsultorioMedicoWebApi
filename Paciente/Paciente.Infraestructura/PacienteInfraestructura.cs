using System.Data;
using DESI.ConsultorioMedico.Common.Data;
using DESI.ConsultorioMedico.Common.Mensajes;
using DESI.ConsultorioMedico.Paciente.Mensajes;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DESI.ConsultorioMedico.Paciente.Infraestructura;

/// <summary>
/// Implementación ADO.NET del repositorio de Paciente (acceso vía procedimientos almacenados).
/// </summary>
public class PacienteInfraestructura : BaseRepository, IPacienteRepository
{
    private const string SpGetAllPacientes = "GetAllPacientes";
    private const string SpGetPacienteById = "GetPacienteById";
    private const string SpSaveOrUpdatePaciente = "SaveOrUpdatePaciente";

    public PacienteInfraestructura(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<PacienteData>> GetAllPacientes()
    {
        return await ExecuteReaderAsync(SpGetAllPacientes, CommandType.StoredProcedure, MapPacienteData);
    }

    public async Task<PacienteData?> GetPacienteById(int id)
    {
        var parameters = new[]
        {
            new SqlParameter("@Id", SqlDbType.Int) { Value = id }
        };

        var results = await ExecuteReaderAsync(SpGetPacienteById, CommandType.StoredProcedure, MapPacienteData, parameters);
        return results.FirstOrDefault();
    }

    public async Task<OperationResult<int>> SaveOrUpdatePaciente(GuardarPacienteRequest request)
    {
        var parameters = new[]
        {
            new SqlParameter("@Id", SqlDbType.Int) { Value = request.Id },
            new SqlParameter("@Nombre", SqlDbType.NVarChar, 50) { Value = request.Nombre },
            new SqlParameter("@ApellidoPaterno", SqlDbType.NVarChar, 50) { Value = request.ApellidoPaterno },
            new SqlParameter("@ApellidoMaterno", SqlDbType.NVarChar, 50) { Value = (object?)request.ApellidoMaterno ?? DBNull.Value },
            new SqlParameter("@Email", SqlDbType.NVarChar, 50) { Value = (object?)request.Email ?? DBNull.Value },
            new SqlParameter("@TelefonoCel", SqlDbType.NVarChar, 100) { Value = (object?)request.TelefonoCel ?? DBNull.Value },
            new SqlParameter("@Comentarios", SqlDbType.NVarChar, 1000) { Value = (object?)request.Comentarios ?? DBNull.Value },
            new SqlParameter("@Sexo", SqlDbType.NVarChar, 15) { Value = (object?)request.Sexo ?? DBNull.Value },
            new SqlParameter("@FechaNacimiento", SqlDbType.DateTime) { Value = (object?)request.FechaNacimiento ?? DBNull.Value },
            new SqlParameter("@Estatus", SqlDbType.Bit) { Value = request.Estatus },
        };

        var id = await ExecuteNonQueryAsync(SpSaveOrUpdatePaciente, parameters);
        return OperationResult<int>.Success(id);
    }

    private static PacienteData MapPacienteData(SqlDataReader reader)
    {
        return new PacienteData
        {
            Id = reader.GetInt32Safe("Id"),
            Nombre = reader.GetStringSafe("Nombre") ?? string.Empty,
            ApellidoPaterno = reader.GetStringSafe("ApellidoPaterno") ?? string.Empty,
            ApellidoMaterno = reader.GetStringSafe("ApellidoMaterno"),
            Email = reader.GetStringSafe("Email"),
            FechaAlta = reader.GetDateTimeSafe("FechaAlta"),
            TelefonoCel = reader.GetStringSafe("TelefonoCel"),
            Comentarios = reader.GetStringSafe("Comentarios"),
            Estatus = reader.GetBoolSafe("Estatus"),
            Edad = reader.GetInt32NullableSafe("Edad"),
            Sexo = reader.GetStringSafe("Sexo"),
            FechaNacimiento = reader.GetDateTimeSafe("FechaNacimiento"),
            CreatedBY = reader.GetStringSafe("CreatedBY"),
            CreatedDT = reader.GetDateTimeSafe("CreatedDT"),
            UpdatedBY = reader.GetStringSafe("UpdatedBY"),
            UpdatedDT = reader.GetDateTimeSafe("UpdatedDT"),
        };
    }
}
