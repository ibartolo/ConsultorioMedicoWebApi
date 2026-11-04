using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using Serilog;

namespace ConsultorioMedicoWebApi.Services
{
    public class DatosInstitucionService
    {
        private const string CarpetaRaiz = "Attachment";
        private const long MaxLogoBytes = 2L * 1024 * 1024;
        private static readonly string[] ExtensionesLogo = { ".png", ".jpg", ".jpeg" };

        private readonly DbWrapper _dbWrapper;

        public DatosInstitucionService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<DatosInstitucion> ObtenerDatosInstitucion(int empresaId, string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerDatosInstitucion(empresaId, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerDatosInstitucion");
                return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = "Ocurrió un error al obtener los datos de la institución." };
            }
        }

        public ModelResponse<DatosInstitucion> GuardarDatosInstitucion(DatosInstitucion datos, int empresaId, string usuario)
        {
            try
            {
                var error = DatosInstitucionValidator.Validar(datos);
                if (error != null)
                    return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = error };

                // La institución del tenant: el Id SIEMPRE se fija desde el claim
                // (nunca del cliente). El tenant es la institución (DatosInstitucion.Id).
                datos.Id = empresaId;
                return _dbWrapper.GuardarDatosInstitucion(datos, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en GuardarDatosInstitucion");
                return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = "Ocurrió un error al guardar los datos de la institución." };
            }
        }

        /// <summary>Guarda el logo de la institución en Attachment/{consultorioId}/institucion y actualiza LogoRuta.</summary>
        public ModelResponse<DatosInstitucion> GuardarLogo(int empresaId, HttpFileCollection files, string usuario)
        {
            var mr = new ModelResponse<DatosInstitucion>();
            string rutaAbsoluta = null;
            try
            {
                if (empresaId <= 0)
                    return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = "Institución no válida." };

                if (files == null || files.Count == 0)
                    return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = "Debe seleccionar un logo." };

                var file = files[0];
                if (file == null || file.ContentLength == 0)
                    return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = "El archivo del logo está vacío." };

                var extension = Path.GetExtension(Path.GetFileName(file.FileName)).ToLowerInvariant();
                if (Array.IndexOf(ExtensionesLogo, extension) < 0)
                    return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = "Solo se permiten logos PNG, JPG o JPEG." };

                if (file.ContentLength > MaxLogoBytes)
                    return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = "El logo excede el tamaño máximo permitido (2 MB)." };

                var actual = _dbWrapper.ObtenerDatosInstitucion(empresaId, usuario);
                if (actual == null || !actual.IsSuccess || actual.Response == null)
                    return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = "Institución no encontrada." };

                var nombreFisico = Guid.NewGuid().ToString() + extension;
                // Ruta del logo por tenant: /Attachment/{consultorioId}/institucion/{guid}.ext
                var rutaRelativa = "/" + CarpetaRaiz + "/" + empresaId + "/institucion/" + nombreFisico;
                rutaAbsoluta = HostingEnvironment.MapPath("~" + rutaRelativa);
                if (string.IsNullOrEmpty(rutaAbsoluta))
                    return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = "No se pudo resolver la ruta del logo." };

                var directorio = Path.GetDirectoryName(rutaAbsoluta);
                if (!Directory.Exists(directorio))
                    Directory.CreateDirectory(directorio);

                file.SaveAs(rutaAbsoluta);

                if (!_dbWrapper.ActualizarLogoInstitucion(empresaId, rutaRelativa))
                {
                    if (File.Exists(rutaAbsoluta)) File.Delete(rutaAbsoluta);
                    return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = "No fue posible guardar el logo." };
                }

                EliminarLogoAnterior(actual.Response.LogoRuta, empresaId);
                actual.Response.LogoRuta = rutaRelativa;

                mr.IsSuccess = true;
                mr.Response = actual.Response;
                mr.Message = "Logo actualizado correctamente.";
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(rutaAbsoluta) && File.Exists(rutaAbsoluta))
                {
                    try { File.Delete(rutaAbsoluta); } catch { }
                }
                Log.Error(ex, "Error en GuardarLogo de institución {Id}", empresaId);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al guardar el logo.";
            }
            return mr;
        }

        public byte[] ObtenerBytesLogo(DatosInstitucion institucion)
        {
            if (institucion == null || string.IsNullOrWhiteSpace(institucion.LogoRuta)) return null;

            string rutaAbsoluta;
            if (institucion.LogoRuta.StartsWith("/"))
                rutaAbsoluta = HostingEnvironment.MapPath("~" + institucion.LogoRuta);
            else if (Path.IsPathRooted(institucion.LogoRuta))
                rutaAbsoluta = institucion.LogoRuta;
            else
                rutaAbsoluta = HostingEnvironment.MapPath("~/" + institucion.LogoRuta);

            if (string.IsNullOrEmpty(rutaAbsoluta) || !File.Exists(rutaAbsoluta)) return null;
            return File.ReadAllBytes(rutaAbsoluta);
        }

        private void EliminarLogoAnterior(string ruta, int empresaId)
        {
            var prefijo = "/" + CarpetaRaiz + "/" + empresaId + "/institucion/";
            if (string.IsNullOrWhiteSpace(ruta) || !ruta.StartsWith(prefijo)) return;
            try
            {
                var abs = HostingEnvironment.MapPath("~" + ruta);
                if (!string.IsNullOrEmpty(abs) && File.Exists(abs)) File.Delete(abs);
            }
            catch { }
        }
    }
}
