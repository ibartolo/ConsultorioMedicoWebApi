using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using ConsultorioMedicoWebApi.Helpers;
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
                Log.Information("DatosInstitucionService.ObtenerDatosInstitucion ENTRADA: {Json}", LogJson.ToJson(new { empresaId, usuario }));

                Log.Debug("DatosInstitucionService.ObtenerDatosInstitucion DB_REQUEST: {Json}", LogJson.ToJson(new { empresaId, usuario }));
                var result = _dbWrapper.ObtenerDatosInstitucion(empresaId, usuario);
                Log.Debug("DatosInstitucionService.ObtenerDatosInstitucion DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("DatosInstitucionService.ObtenerDatosInstitucion RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DatosInstitucionService.ObtenerDatosInstitucion ERROR: {Message}", ex.Message);
                return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = "Ocurrió un error al obtener los datos de la institución." };
            }
        }

        public ModelResponse<DatosInstitucion> GuardarDatosInstitucion(DatosInstitucion datos, int empresaId, string usuario)
        {
            try
            {
                Log.Information("DatosInstitucionService.GuardarDatosInstitucion ENTRADA: {Json}", LogJson.ToJson(new { datos, empresaId, usuario }));

                var error = DatosInstitucionValidator.Validar(datos);
                if (error != null)
                {
                    Log.Warning("DatosInstitucionService.GuardarDatosInstitucion VALIDACIÓN: {Mensaje}", error);
                    return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = error };
                }

                // La institución del tenant: el Id SIEMPRE se fija desde el claim
                // (nunca del cliente). El tenant es la institución (DatosInstitucion.Id).
                datos.Id = empresaId;

                Log.Debug("DatosInstitucionService.GuardarDatosInstitucion DB_REQUEST: {Json}", LogJson.ToJson(new { datos, usuario }));
                var result = _dbWrapper.GuardarDatosInstitucion(datos, usuario);
                Log.Debug("DatosInstitucionService.GuardarDatosInstitucion DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("DatosInstitucionService.GuardarDatosInstitucion RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DatosInstitucionService.GuardarDatosInstitucion ERROR: {Message}", ex.Message);
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
                Log.Information("DatosInstitucionService.GuardarLogo ENTRADA: {Json}", LogJson.ToJson(new { empresaId, usuario, FileCount = files?.Count }));

                if (empresaId <= 0)
                {
                    Log.Warning("DatosInstitucionService.GuardarLogo VALIDACIÓN: {Mensaje}", "Institución no válida.");
                    return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = "Institución no válida." };
                }

                if (files == null || files.Count == 0)
                {
                    Log.Warning("DatosInstitucionService.GuardarLogo VALIDACIÓN: {Mensaje}", "Debe seleccionar un logo.");
                    return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = "Debe seleccionar un logo." };
                }

                var file = files[0];
                if (file == null || file.ContentLength == 0)
                {
                    Log.Warning("DatosInstitucionService.GuardarLogo VALIDACIÓN: {Mensaje}", "El archivo del logo está vacío.");
                    return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = "El archivo del logo está vacío." };
                }

                var extension = Path.GetExtension(Path.GetFileName(file.FileName)).ToLowerInvariant();
                if (Array.IndexOf(ExtensionesLogo, extension) < 0)
                {
                    Log.Warning("DatosInstitucionService.GuardarLogo VALIDACIÓN: {Mensaje}", "Solo se permiten logos PNG, JPG o JPEG.");
                    return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = "Solo se permiten logos PNG, JPG o JPEG." };
                }

                if (file.ContentLength > MaxLogoBytes)
                {
                    Log.Warning("DatosInstitucionService.GuardarLogo VALIDACIÓN: {Mensaje}", "El logo excede el tamaño máximo permitido (2 MB).");
                    return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = "El logo excede el tamaño máximo permitido (2 MB)." };
                }

                Log.Debug("DatosInstitucionService.GuardarLogo DB_REQUEST (obtener institución): {Json}", LogJson.ToJson(new { empresaId, usuario }));
                var actual = _dbWrapper.ObtenerDatosInstitucion(empresaId, usuario);
                Log.Debug("DatosInstitucionService.GuardarLogo DB_RESPONSE (obtener institución): {Json}", LogJson.ToJson(actual));

                if (actual == null || !actual.IsSuccess || actual.Response == null)
                {
                    Log.Warning("DatosInstitucionService.GuardarLogo VALIDACIÓN: {Mensaje}", "Institución no encontrada.");
                    return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = "Institución no encontrada." };
                }

                var nombreFisico = Guid.NewGuid().ToString() + extension;
                // Ruta del logo por tenant: /Attachment/{consultorioId}/institucion/{guid}.ext
                var rutaRelativa = "/" + CarpetaRaiz + "/" + empresaId + "/institucion/" + nombreFisico;
                rutaAbsoluta = HostingEnvironment.MapPath("~" + rutaRelativa);
                if (string.IsNullOrEmpty(rutaAbsoluta))
                {
                    Log.Warning("DatosInstitucionService.GuardarLogo VALIDACIÓN: {Mensaje}", "No se pudo resolver la ruta del logo.");
                    return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = "No se pudo resolver la ruta del logo." };
                }

                var directorio = Path.GetDirectoryName(rutaAbsoluta);
                if (!Directory.Exists(directorio))
                    Directory.CreateDirectory(directorio);

                file.SaveAs(rutaAbsoluta);
                Log.Debug("DatosInstitucionService.GuardarLogo ARCHIVO: {Json}", LogJson.ToJson(new { NombreOriginal = Path.GetFileName(file.FileName), file.ContentLength, extension }));

                Log.Debug("DatosInstitucionService.GuardarLogo DB_REQUEST (actualizar logo): {Json}", LogJson.ToJson(new { empresaId, rutaRelativa }));
                var actualizado = _dbWrapper.ActualizarLogoInstitucion(empresaId, rutaRelativa);
                Log.Debug("DatosInstitucionService.GuardarLogo DB_RESPONSE (actualizar logo): {Json}", LogJson.ToJson(actualizado));

                if (!actualizado)
                {
                    if (File.Exists(rutaAbsoluta)) File.Delete(rutaAbsoluta);
                    Log.Warning("DatosInstitucionService.GuardarLogo VALIDACIÓN: {Mensaje}", "No fue posible guardar el logo.");
                    return new ModelResponse<DatosInstitucion> { IsSuccess = false, Message = "No fue posible guardar el logo." };
                }

                EliminarLogoAnterior(actual.Response.LogoRuta, empresaId);
                actual.Response.LogoRuta = rutaRelativa;

                mr.IsSuccess = true;
                mr.Response = actual.Response;
                mr.Message = "Logo actualizado correctamente.";
                Log.Information("DatosInstitucionService.GuardarLogo RESULTADO: IsSuccess=True, Message=Logo actualizado correctamente., Id={Id}", actual.Response.Id);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(rutaAbsoluta) && File.Exists(rutaAbsoluta))
                {
                    try { File.Delete(rutaAbsoluta); } catch { }
                }
                Log.Error(ex, "DatosInstitucionService.GuardarLogo ERROR: {Message}", ex.Message);
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

            if (string.IsNullOrEmpty(rutaAbsoluta) || !File.Exists(rutaAbsoluta))
            {
                Log.Warning("DatosInstitucionService.ObtenerBytesLogo VALIDACIÓN: No existe el archivo del logo en disco. Ruta={Ruta}", rutaAbsoluta);
                return null;
            }

            var bytes = File.ReadAllBytes(rutaAbsoluta);
            Log.Debug("DatosInstitucionService.ObtenerBytesLogo ARCHIVO: {Json}", LogJson.ToJson(new { institucion.Id, Longitud = bytes?.Length }));
            return bytes;
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
