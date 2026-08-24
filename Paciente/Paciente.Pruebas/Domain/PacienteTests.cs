using Xunit;
using Dominio = DESI.ConsultorioMedico.Paciente.Dominio;

namespace DESI.ConsultorioMedico.Paciente.Pruebas.Domain;

public class PacienteTests
{
    [Fact]
    public void Create_RetornaNuevaInstancia()
    {
        var paciente = Dominio.Paciente.Create();

        Assert.NotNull(paciente);
    }

    [Fact]
    public void Set_Fluent_RetornaMismaInstancia()
    {
        var paciente = Dominio.Paciente.Create();

        Assert.Same(paciente, paciente.SetId(1));
        Assert.Same(paciente, paciente.SetNombre("Ana"));
        Assert.Same(paciente, paciente.SetApellidoPaterno("López"));
    }

    [Fact]
    public void SetNombre_Vacio_LanzaArgumentException()
    {
        var paciente = Dominio.Paciente.Create();

        Assert.Throws<ArgumentException>(() => paciente.SetNombre(""));
    }

    [Fact]
    public void SetNombre_Nulo_LanzaArgumentException()
    {
        var paciente = Dominio.Paciente.Create();

        Assert.Throws<ArgumentException>(() => paciente.SetNombre(null!));
    }

    [Fact]
    public void SetNombre_Excede50_LanzaArgumentException()
    {
        var paciente = Dominio.Paciente.Create();
        var nombreLargo = new string('a', 51);

        Assert.Throws<ArgumentException>(() => paciente.SetNombre(nombreLargo));
    }

    [Fact]
    public void SetApellidoPaterno_Vacio_LanzaArgumentException()
    {
        var paciente = Dominio.Paciente.Create();

        Assert.Throws<ArgumentException>(() => paciente.SetApellidoPaterno(""));
    }

    [Fact]
    public void SetComentarios_Excede1000_LanzaArgumentException()
    {
        var paciente = Dominio.Paciente.Create();
        var comentariosLargos = new string('a', 1001);

        Assert.Throws<ArgumentException>(() => paciente.SetComentarios(comentariosLargos));
    }

    [Fact]
    public void Set_AsignaValoresCorrectos()
    {
        var paciente = Dominio.Paciente.Create()
            .SetId(7)
            .SetNombre("Ana")
            .SetApellidoPaterno("López")
            .SetEstatus(true)
            .SetEdad(30);

        Assert.Equal(7, paciente.Id);
        Assert.Equal("Ana", paciente.Nombre);
        Assert.Equal("López", paciente.ApellidoPaterno);
        Assert.True(paciente.Estatus);
        Assert.Equal(30, paciente.Edad!.Value);
    }
}
