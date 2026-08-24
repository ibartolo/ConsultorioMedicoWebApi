using DESI.ConsultorioMedico.Paciente.Dominio.ValueObjects;

namespace DESI.ConsultorioMedico.Paciente.Dominio;

/// <summary>
/// Entidad Paciente. Se construye mediante Factory Method (<see cref="Create"/>) y
/// se configura con Fluent Interface (métodos <c>Set…</c> que retornan <c>this</c>).
/// </summary>
public class Paciente : Entity<int>
{
    private Paciente()
    {
    }

    public string Nombre { get; private set; } = string.Empty;
    public string ApellidoPaterno { get; private set; } = string.Empty;
    public string? ApellidoMaterno { get; private set; }
    public Email? Email { get; private set; }
    public DateTime? FechaAlta { get; private set; }
    public Telefono? TelefonoCel { get; private set; }
    public string? Comentarios { get; private set; }
    public bool Estatus { get; private set; }
    public int? Edad { get; private set; }
    public Sexo? Sexo { get; private set; }
    public DateTime? FechaNacimiento { get; private set; }
    public string? CreatedBY { get; private set; }
    public DateTime? CreatedDT { get; private set; }
    public string? UpdatedBY { get; private set; }
    public DateTime? UpdatedDT { get; private set; }

    /// <summary>
    /// Crea una nueva instancia de <see cref="Paciente"/>.
    /// </summary>
    public static Paciente Create() => new Paciente();

    public Paciente SetId(int id)
    {
        Id = id;
        return this;
    }

    public Paciente SetNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        if (nombre.Length > 50)
            throw new ArgumentException("El nombre no puede exceder 50 caracteres.", nameof(nombre));
        Nombre = nombre;
        return this;
    }

    public Paciente SetApellidoPaterno(string apellidoPaterno)
    {
        if (string.IsNullOrWhiteSpace(apellidoPaterno))
            throw new ArgumentException("El apellido paterno es obligatorio.", nameof(apellidoPaterno));
        if (apellidoPaterno.Length > 50)
            throw new ArgumentException("El apellido paterno no puede exceder 50 caracteres.", nameof(apellidoPaterno));
        ApellidoPaterno = apellidoPaterno;
        return this;
    }

    public Paciente SetApellidoMaterno(string? apellidoMaterno)
    {
        if (apellidoMaterno is not null && apellidoMaterno.Length > 50)
            throw new ArgumentException("El apellido materno no puede exceder 50 caracteres.", nameof(apellidoMaterno));
        ApellidoMaterno = apellidoMaterno;
        return this;
    }

    public Paciente SetEmail(Email? email)
    {
        Email = email;
        return this;
    }

    public Paciente SetFechaAlta(DateTime? fechaAlta)
    {
        FechaAlta = fechaAlta;
        return this;
    }

    public Paciente SetTelefonoCel(Telefono? telefonoCel)
    {
        TelefonoCel = telefonoCel;
        return this;
    }

    public Paciente SetComentarios(string? comentarios)
    {
        if (comentarios is not null && comentarios.Length > 1000)
            throw new ArgumentException("Los comentarios no pueden exceder 1000 caracteres.", nameof(comentarios));
        Comentarios = comentarios;
        return this;
    }

    public Paciente SetEstatus(bool estatus)
    {
        Estatus = estatus;
        return this;
    }

    public Paciente SetEdad(int? edad)
    {
        Edad = edad;
        return this;
    }

    public Paciente SetSexo(Sexo? sexo)
    {
        Sexo = sexo;
        return this;
    }

    public Paciente SetFechaNacimiento(DateTime? fechaNacimiento)
    {
        FechaNacimiento = fechaNacimiento;
        return this;
    }

    public Paciente SetCreatedBY(string? createdBY)
    {
        if (createdBY is not null && createdBY.Length > 25)
            throw new ArgumentException("CreatedBY no puede exceder 25 caracteres.", nameof(createdBY));
        CreatedBY = createdBY;
        return this;
    }

    public Paciente SetCreatedDT(DateTime? createdDT)
    {
        CreatedDT = createdDT;
        return this;
    }

    public Paciente SetUpdatedBY(string? updatedBY)
    {
        if (updatedBY is not null && updatedBY.Length > 25)
            throw new ArgumentException("UpdatedBY no puede exceder 25 caracteres.", nameof(updatedBY));
        UpdatedBY = updatedBY;
        return this;
    }

    public Paciente SetUpdatedDT(DateTime? updatedDT)
    {
        UpdatedDT = updatedDT;
        return this;
    }
}
