using DESI.ConsultorioMedico.Paciente.Dominio.ValueObjects;
using Dominio = DESI.ConsultorioMedico.Paciente.Dominio;

namespace DESI.ConsultorioMedico.Paciente.Mensajes;

/// <summary>
/// Mapeos estáticos entre DTO crudo, entidad y DTO de respuesta.
/// </summary>
public static class Mapp
{
    /// <summary>
    /// Convierte el DTO crudo a la entidad <see cref="Dominio.Paciente"/> (ejercita la validación Fluent/Factory).
    /// </summary>
    public static Dominio.Paciente MapToPaciente(PacienteData data)
    {
        return Dominio.Paciente.Create()
            .SetId(data.Id)
            .SetNombre(data.Nombre)
            .SetApellidoPaterno(data.ApellidoPaterno)
            .SetApellidoMaterno(data.ApellidoMaterno)
            .SetEmail(data.Email is null ? null : new Email(data.Email))
            .SetFechaAlta(data.FechaAlta)
            .SetTelefonoCel(data.TelefonoCel is null ? null : new Telefono(data.TelefonoCel))
            .SetComentarios(data.Comentarios)
            .SetEstatus(data.Estatus)
            .SetEdad(data.Edad)
            .SetSexo(data.Sexo is null ? null : new Sexo(data.Sexo))
            .SetFechaNacimiento(data.FechaNacimiento)
            .SetCreatedBY(data.CreatedBY)
            .SetCreatedDT(data.CreatedDT)
            .SetUpdatedBY(data.UpdatedBY)
            .SetUpdatedDT(data.UpdatedDT);
    }

    /// <summary>
    /// Convierte una colección de DTOs crudos a entidades.
    /// </summary>
    public static List<Dominio.Paciente> MapToPacientes(IEnumerable<PacienteData> data)
        => data.Select(MapToPaciente).ToList();

    /// <summary>
    /// Convierte una entidad a DTO de respuesta, componiendo el nombre completo.
    /// </summary>
    public static PacienteResponse MapToPacienteResponse(Dominio.Paciente paciente)
    {
        return new PacienteResponse
        {
            Id = paciente.Id,
            Nombre = paciente.Nombre,
            ApellidoPaterno = paciente.ApellidoPaterno,
            ApellidoMaterno = paciente.ApellidoMaterno,
            NombreCompleto = $"{paciente.Nombre} {paciente.ApellidoPaterno} {paciente.ApellidoMaterno}".Trim(),
            Sexo = paciente.Sexo?.Value,
            Edad = paciente.Edad ?? 0,
            TelefonoCel = paciente.TelefonoCel?.Value,
            Email = paciente.Email?.Value,
            Estatus = paciente.Estatus,
            FechaNacimiento = paciente.FechaNacimiento,
            Comentarios = paciente.Comentarios,
        };
    }

    /// <summary>
    /// Convierte una lista de entidades a un DTO con la lista de respuestas.
    /// </summary>
    public static GetPacientesResponse MapToPacientesResponse(List<Dominio.Paciente> pacientes)
    {
        return new GetPacientesResponse
        {
            Pacientes = pacientes.Select(MapToPacienteResponse).ToList()
        };
    }
}
