namespace ConsultorioMedicoEntities.Catalogos
{
    /// <summary>
    /// Validaciones de negocio de DatosFiscales, compartidas entre MVC y WebApi.
    /// </summary>
    public static class DatosFiscalesValidator
    {
        public static string Validar(DatosFiscales d)
        {
            if (d == null) return "Los datos fiscales son inválidos.";

            if (string.IsNullOrWhiteSpace(d.RazonSocial)) return "La razón social es requerida.";
            if (Largo(d.RazonSocial) > 100) return "La razón social no puede exceder 100 caracteres.";
            if (string.IsNullOrWhiteSpace(d.RFC)) return "El RFC es requerido.";
            if (Largo(d.RFC) > 100) return "El RFC no puede exceder 100 caracteres.";

            if (Largo(d.Telefono1) > 50) return "El teléfono 1 no puede exceder 50 caracteres.";
            if (Largo(d.Telefono2) > 50) return "El teléfono 2 no puede exceder 50 caracteres.";
            if (Largo(d.Email) > 100) return "El correo electrónico no puede exceder 100 caracteres.";
            if (!string.IsNullOrWhiteSpace(d.Email) && !d.Email.Contains("@")) return "El correo electrónico no es válido.";
            if (Largo(d.Calle) > 50) return "La calle no puede exceder 50 caracteres.";
            if (Largo(d.NoInterior) > 10) return "El número interior no puede exceder 10 caracteres.";
            if (Largo(d.NoExterior) > 10) return "El número exterior no puede exceder 10 caracteres.";
            if (Largo(d.Colonia) > 50) return "La colonia no puede exceder 50 caracteres.";
            if (Largo(d.Deleacion) > 50) return "La delegación no puede exceder 50 caracteres.";
            if (Largo(d.Municipio) > 50) return "El municipio no puede exceder 50 caracteres.";
            if (Largo(d.Estado) > 50) return "El estado no puede exceder 50 caracteres.";
            if (Largo(d.Pais) > 50) return "El país no puede exceder 50 caracteres.";

            if (d.CP.HasValue && (d.CP.Value < 0 || d.CP.Value > 99999))
                return "El código postal no es válido.";

            if (d.DatosInstitucionID.HasValue && d.DatosInstitucionID.Value <= 0)
                return "La institución seleccionada no es válida.";

            return null;
        }

        private static int Largo(string valor)
        {
            return string.IsNullOrEmpty(valor) ? 0 : valor.Length;
        }
    }
}
