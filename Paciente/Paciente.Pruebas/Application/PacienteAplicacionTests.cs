using DESI.ConsultorioMedico.Paciente.Aplicacion;
using DESI.ConsultorioMedico.Paciente.Mensajes;
using DESI.ConsultorioMedico.Paciente.Pruebas.Fakes;
using Xunit;

namespace DESI.ConsultorioMedico.Paciente.Pruebas.Application;

public class PacienteAplicacionTests
{
    private static PacienteData CrearData(int id, string nombre = "Ana") => new()
    {
        Id = id,
        Nombre = nombre,
        ApellidoPaterno = "López",
        ApellidoMaterno = "García",
        Email = "ana@example.com",
        TelefonoCel = "5512345678",
        Estatus = true,
        Edad = 30,
        Sexo = "Femenino"
    };

    [Fact]
    public async Task ObtenerTodos_ConDatos_RetornaExito()
    {
        var repo = new FakePacienteRepository(new List<PacienteData> { CrearData(1), CrearData(2, "Luis") });
        var aplicacion = new PacienteAplicacion(repo);

        var result = await aplicacion.ObtenerTodos();

        Assert.True(result.IsSuccess);
        Assert.Equal("Pacientes obtenidos correctamente", result.Message);
        Assert.NotNull(result.Response);
        Assert.Equal(2, result.Response!.Pacientes.Count);
    }

    [Fact]
    public async Task ObtenerTodos_SinDatos_RetornaListaVacia()
    {
        var repo = new FakePacienteRepository();
        var aplicacion = new PacienteAplicacion(repo);

        var result = await aplicacion.ObtenerTodos();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Response);
        Assert.Empty(result.Response!.Pacientes);
    }

    [Fact]
    public async Task ObtenerPorId_Existente_RetornaPaciente()
    {
        var repo = new FakePacienteRepository(new List<PacienteData> { CrearData(5) });
        var aplicacion = new PacienteAplicacion(repo);

        var result = await aplicacion.ObtenerPorId(5);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Response);
        Assert.Equal(5, result.Response!.Id);
    }

    [Fact]
    public async Task ObtenerPorId_Inexistente_RetornaFallo()
    {
        var repo = new FakePacienteRepository();
        var aplicacion = new PacienteAplicacion(repo);

        var result = await aplicacion.ObtenerPorId(9999);

        Assert.False(result.IsSuccess);
        Assert.Equal("Paciente no encontrado", result.Message);
        Assert.Null(result.Response);
    }

    [Fact]
    public async Task Guardar_IdCero_CreaPaciente()
    {
        var repo = new FakePacienteRepository();
        var aplicacion = new PacienteAplicacion(repo);

        var result = await aplicacion.Guardar(new GuardarPacienteRequest
        {
            Id = 0,
            Nombre = "Ana",
            ApellidoPaterno = "López",
            Estatus = true
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("Paciente creado exitosamente", result.Message);
        Assert.True(result.Response > 0);
    }

    [Fact]
    public async Task Guardar_IdMayorCero_ActualizaPaciente()
    {
        var repo = new FakePacienteRepository();
        var aplicacion = new PacienteAplicacion(repo);

        var result = await aplicacion.Guardar(new GuardarPacienteRequest
        {
            Id = 10,
            Nombre = "Ana",
            ApellidoPaterno = "López",
            Estatus = true
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("Paciente actualizado exitosamente", result.Message);
        Assert.Equal(10, result.Response);
    }
}
