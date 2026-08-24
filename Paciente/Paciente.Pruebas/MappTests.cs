using DESI.ConsultorioMedico.Paciente.Mensajes;
using Xunit;

namespace DESI.ConsultorioMedico.Paciente.Pruebas;

public class MappTests
{
    private static PacienteData CrearData() => new()
    {
        Id = 1,
        Nombre = "Ana",
        ApellidoPaterno = "López",
        ApellidoMaterno = "García",
        Email = "ana@example.com",
        TelefonoCel = "5512345678",
        Estatus = true,
        Edad = 30,
        Sexo = "Femenino",
        FechaNacimiento = new DateTime(1995, 5, 10),
        Comentarios = "Paciente regular"
    };

    [Fact]
    public void MapToPaciente_ConvierteCrudoAEntidad()
    {
        var data = CrearData();

        var paciente = Mapp.MapToPaciente(data);

        Assert.Equal(1, paciente.Id);
        Assert.Equal("Ana", paciente.Nombre);
        Assert.Equal("López", paciente.ApellidoPaterno);
        Assert.Equal("García", paciente.ApellidoMaterno);
        Assert.Equal("ana@example.com", paciente.Email?.Value);
        Assert.Equal("5512345678", paciente.TelefonoCel?.Value);
        Assert.Equal("Femenino", paciente.Sexo?.Value);
        Assert.True(paciente.Estatus);
        Assert.Equal(30, paciente.Edad!.Value);
    }

    [Fact]
    public void MapToPacienteResponse_ComponeNombreCompleto()
    {
        var data = CrearData();
        var paciente = Mapp.MapToPaciente(data);

        var response = Mapp.MapToPacienteResponse(paciente);

        Assert.Equal("Ana López García", response.NombreCompleto);
        Assert.Equal(1, response.Id);
        Assert.Equal("Femenino", response.Sexo);
        Assert.Equal("ana@example.com", response.Email);
    }

    [Fact]
    public void MapToPacienteResponse_EdadNula_CoalesceCero()
    {
        var data = CrearData();
        data.Edad = null;
        var paciente = Mapp.MapToPaciente(data);

        var response = Mapp.MapToPacienteResponse(paciente);

        Assert.Equal(0, response.Edad);
    }
}
