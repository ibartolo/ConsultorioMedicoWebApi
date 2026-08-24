using Xunit;
using VOs = DESI.ConsultorioMedico.Paciente.Dominio.ValueObjects;

namespace DESI.ConsultorioMedico.Paciente.Pruebas.Domain;

public class ValueObjectsTests
{
    [Fact]
    public void Email_FormatoInvalido_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new VOs.Email("ana@"));
    }

    [Fact]
    public void Email_Nulo_NoLanza()
    {
        var email = new VOs.Email(null);

        Assert.Null(email.Value);
    }

    [Fact]
    public void Email_Excede50_LanzaArgumentException()
    {
        var largo = new string('a', 51) + "@example.com";

        Assert.Throws<ArgumentException>(() => new VOs.Email(largo));
    }

    [Fact]
    public void Email_Valido_AsignaValor()
    {
        var email = new VOs.Email("ana@example.com");

        Assert.Equal("ana@example.com", email.Value);
    }

    [Fact]
    public void Telefono_Excede100_LanzaArgumentException()
    {
        var largo = new string('1', 101);

        Assert.Throws<ArgumentException>(() => new VOs.Telefono(largo));
    }

    [Fact]
    public void Telefono_Nulo_NoLanza()
    {
        var telefono = new VOs.Telefono(null);

        Assert.Null(telefono.Value);
    }

    [Fact]
    public void Sexo_ValorNoPermitido_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new VOs.Sexo("Otro"));
    }

    [Fact]
    public void Sexo_Nulo_NoLanza()
    {
        var sexo = new VOs.Sexo(null);

        Assert.Null(sexo.Value);
    }

    [Fact]
    public void Sexo_ValoresPermitidos_NoLanzan()
    {
        var masculino = new VOs.Sexo("Masculino");
        var femenino = new VOs.Sexo("Femenino");

        Assert.Equal("Masculino", masculino.Value);
        Assert.Equal("Femenino", femenino.Value);
    }
}
