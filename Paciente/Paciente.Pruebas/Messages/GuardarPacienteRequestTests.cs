using System.ComponentModel.DataAnnotations;
using DESI.ConsultorioMedico.Paciente.Mensajes;
using Xunit;

namespace DESI.ConsultorioMedico.Paciente.Pruebas.Messages;

public class GuardarPacienteRequestTests
{
    private static GuardarPacienteRequest CrearRequestValida() => new()
    {
        Nombre = "Ana",
        ApellidoPaterno = "López",
        Estatus = true
    };

    private static IList<ValidationResult> Validar(GuardarPacienteRequest request)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), resultados, validateAllProperties: true);
        return resultados;
    }

    [Fact]
    public void Sexo_Otro_EsInvalido()
    {
        var request = CrearRequestValida();
        request.Sexo = "Otro";

        var errores = Validar(request);

        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(GuardarPacienteRequest.Sexo)));
    }

    [Fact]
    public void Sexo_Masculino_EsValido()
    {
        var request = CrearRequestValida();
        request.Sexo = "Masculino";

        Assert.Empty(Validar(request));
    }

    [Fact]
    public void Sexo_Femenino_EsValido()
    {
        var request = CrearRequestValida();
        request.Sexo = "Femenino";

        Assert.Empty(Validar(request));
    }

    [Fact]
    public void Sexo_Nulo_EsValido()
    {
        var request = CrearRequestValida();

        Assert.Empty(Validar(request));
    }
}
