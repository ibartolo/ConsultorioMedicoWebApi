namespace DESI.ConsultorioMedico.Common.Mensajes;

/// <summary>
/// Tipos de mensajes del sistema, para acceso tipado al catálogo de mensajes.
/// </summary>
public enum SystemMessageTypes
{
    PacientesObtenidos,
    PacienteNoEncontrado,
    PacienteCreado,
    PacienteActualizado,
    IdentificadorInvalido,
    DatosEntradaInvalidos
}

/// <summary>
/// Catálogo estático de mensajes del sistema (en español).
/// </summary>
public static class SystemMessage
{
    private static readonly Dictionary<SystemMessageTypes, string> Catalogo = new()
    {
        [SystemMessageTypes.PacientesObtenidos] = "Pacientes obtenidos correctamente",
        [SystemMessageTypes.PacienteNoEncontrado] = "Paciente no encontrado",
        [SystemMessageTypes.PacienteCreado] = "Paciente creado exitosamente",
        [SystemMessageTypes.PacienteActualizado] = "Paciente actualizado exitosamente",
        [SystemMessageTypes.IdentificadorInvalido] = "El identificador debe ser mayor que cero.",
        [SystemMessageTypes.DatosEntradaInvalidos] = "Datos de entrada inválidos.",
    };

    /// <summary>
    /// Obtiene el mensaje en español asociado al tipo indicado.
    /// </summary>
    public static string Get(SystemMessageTypes tipo) => Catalogo[tipo];
}
