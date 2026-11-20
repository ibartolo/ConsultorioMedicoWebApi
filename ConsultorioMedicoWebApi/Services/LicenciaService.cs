using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using ConsultorioMedicoWebApi.Helpers;
using Serilog;

namespace ConsultorioMedicoWebApi.Services
{
    /// <summary>
    /// Servicio de licencia y pagos del consultorio (SaaS).
    /// El tenant (empresaId) proviene del claim OAuth, nunca del cliente.
    /// Al subir comprobante se inserta un pago Pendiente y, si la licencia venció,
    /// el SP extiende la vigencia al fin del día. La confirmación es manual.
    /// </summary>
    public class LicenciaService
    {
        private const string CarpetaRaiz = "Attachment";
        private const long MaxComprobanteBytes = 3L * 1024 * 1024;
        private static readonly string[] ExtensionesPermitidas = { ".pdf", ".jpg", ".jpeg", ".png" };

        private readonly DbWrapper _dbWrapper;

        public LicenciaService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<LicenciaDTO> ObtenerLicencia(int empresaId, string usuario)
        {
            try
            {
                Log.Information("LicenciaService.ObtenerLicencia ENTRADA: {Json}", LogJson.ToJson(new { empresaId, usuario }));

                Log.Debug("LicenciaService.ObtenerLicencia DB_REQUEST: {Json}", LogJson.ToJson(new { empresaId, usuario }));
                var result = _dbWrapper.ObtenerLicencia(empresaId);
                Log.Debug("LicenciaService.ObtenerLicencia DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("LicenciaService.ObtenerLicencia RESULTADO: IsSuccess={IsSuccess}, Message={Message}, ConsultorioID={Id}", result?.IsSuccess, result?.Message, result?.Response?.ConsultorioID);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LicenciaService.ObtenerLicencia ERROR: {Message}", ex.Message);
                return new ModelResponse<LicenciaDTO> { IsSuccess = false, Message = "Ocurrió un error al obtener la licencia." };
            }
        }

        public ModelResponse<List<ConsultorioPago>> ObtenerPagosLicencia(int empresaId, string usuario)
        {
            try
            {
                Log.Information("LicenciaService.ObtenerPagosLicencia ENTRADA: {Json}", LogJson.ToJson(new { empresaId, usuario }));

                Log.Debug("LicenciaService.ObtenerPagosLicencia DB_REQUEST: {Json}", LogJson.ToJson(new { empresaId, usuario }));
                var result = _dbWrapper.ObtenerPagosLicencia(empresaId);
                Log.Debug("LicenciaService.ObtenerPagosLicencia DB_RESPONSE: {Json}", LogJson.ToJson(new { Count = result?.Response?.Count, result?.IsSuccess, result?.Message }));

                Log.Information("LicenciaService.ObtenerPagosLicencia RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Count={Count}", result?.IsSuccess, result?.Message, result?.Response?.Count);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LicenciaService.ObtenerPagosLicencia ERROR: {Message}", ex.Message);
                return new ModelResponse<List<ConsultorioPago>> { IsSuccess = false, Message = "Ocurrió un error al obtener los pagos." };
            }
        }

        public ModelResponse<ConsultorioPago> ObtenerPagoLicenciaById(int pagoId, int empresaId, string usuario)
        {
            try
            {
                Log.Information("LicenciaService.ObtenerPagoLicenciaById ENTRADA: {Json}", LogJson.ToJson(new { pagoId, empresaId, usuario }));

                Log.Debug("LicenciaService.ObtenerPagoLicenciaById DB_REQUEST: {Json}", LogJson.ToJson(new { pagoId, empresaId, usuario }));
                var result = _dbWrapper.ObtenerPagoLicenciaById(pagoId, empresaId);
                Log.Debug("LicenciaService.ObtenerPagoLicenciaById DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("LicenciaService.ObtenerPagoLicenciaById RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LicenciaService.ObtenerPagoLicenciaById ERROR: {Message}", ex.Message);
                return new ModelResponse<ConsultorioPago> { IsSuccess = false, Message = "Ocurrió un error al obtener el comprobante." };
            }
        }

        /// <summary>
        /// Guarda un comprobante de pago (PDF/JPG/JPEG/PNG ≤ 3 MB) en
        /// Attachment/{empresaId}/pagos/{guid}.ext y registra un pago Pendiente.
        /// El monto es fijo (PagoMontoAnual). El correo es best-effort (no revierte).
        /// </summary>
        public ModelResponse<ConsultorioPago> GuardarPagoLicencia(int empresaId, string usuario, HttpFileCollection files)
        {
            var mr = new ModelResponse<ConsultorioPago>();
            string rutaAbsoluta = null;
            try
            {
                Log.Information("LicenciaService.GuardarPagoLicencia ENTRADA: {Json}", LogJson.ToJson(new { empresaId, usuario, FileCount = files?.Count }));

                if (empresaId <= 0)
                {
                    Log.Warning("LicenciaService.GuardarPagoLicencia VALIDACIÓN: {Mensaje}", "Consultorio no válido.");
                    return new ModelResponse<ConsultorioPago> { IsSuccess = false, Message = "Consultorio no válido." };
                }

                if (files == null || files.Count == 0)
                {
                    Log.Warning("LicenciaService.GuardarPagoLicencia VALIDACIÓN: {Mensaje}", "Debe seleccionar un comprobante.");
                    return new ModelResponse<ConsultorioPago> { IsSuccess = false, Message = "Debe seleccionar un comprobante." };
                }

                var file = files[0];
                if (file == null || file.ContentLength == 0)
                {
                    Log.Warning("LicenciaService.GuardarPagoLicencia VALIDACIÓN: {Mensaje}", "El comprobante está vacío.");
                    return new ModelResponse<ConsultorioPago> { IsSuccess = false, Message = "El comprobante está vacío." };
                }

                var nombreOriginal = Path.GetFileName(file.FileName);
                if (string.IsNullOrWhiteSpace(nombreOriginal))
                {
                    Log.Warning("LicenciaService.GuardarPagoLicencia VALIDACIÓN: {Mensaje}", "El nombre del comprobante no es válido.");
                    return new ModelResponse<ConsultorioPago> { IsSuccess = false, Message = "El nombre del comprobante no es válido." };
                }

                var extension = Path.GetExtension(nombreOriginal).ToLowerInvariant();
                if (Array.IndexOf(ExtensionesPermitidas, extension) < 0)
                {
                    Log.Warning("LicenciaService.GuardarPagoLicencia VALIDACIÓN: {Mensaje}", "Solo se permiten comprobantes PDF, JPG, JPEG o PNG.");
                    return new ModelResponse<ConsultorioPago> { IsSuccess = false, Message = "Solo se permiten comprobantes PDF, JPG, JPEG o PNG." };
                }

                if (file.ContentLength > MaxComprobanteBytes)
                {
                    Log.Warning("LicenciaService.GuardarPagoLicencia VALIDACIÓN: {Mensaje}", "El comprobante excede el tamaño máximo permitido (3 MB).");
                    return new ModelResponse<ConsultorioPago> { IsSuccess = false, Message = "El comprobante excede el tamaño máximo permitido (3 MB)." };
                }

                var nombreFisico = Guid.NewGuid().ToString() + extension;
                var rutaRelativa = "/" + CarpetaRaiz + "/" + empresaId + "/pagos/" + nombreFisico;
                rutaAbsoluta = HostingEnvironment.MapPath("~" + rutaRelativa);
                if (string.IsNullOrEmpty(rutaAbsoluta))
                {
                    Log.Warning("LicenciaService.GuardarPagoLicencia VALIDACIÓN: {Mensaje}", "No se pudo resolver la ruta de almacenamiento.");
                    return new ModelResponse<ConsultorioPago> { IsSuccess = false, Message = "No se pudo resolver la ruta de almacenamiento." };
                }

                var directorio = Path.GetDirectoryName(rutaAbsoluta);
                if (!Directory.Exists(directorio))
                    Directory.CreateDirectory(directorio);

                file.SaveAs(rutaAbsoluta);
                Log.Debug("LicenciaService.GuardarPagoLicencia ARCHIVO: {Json}", LogJson.ToJson(new { NombreOriginal = nombreOriginal, file.ContentLength, extension }));

                var monto = ObtenerMontoAnual();

                Log.Debug("LicenciaService.GuardarPagoLicencia DB_REQUEST: {Json}", LogJson.ToJson(new { empresaId, nombreOriginal, rutaRelativa, monto, usuario }));
                var pagoId = _dbWrapper.GuardarPagoLicencia(empresaId, nombreOriginal, rutaRelativa, monto, usuario);
                Log.Debug("LicenciaService.GuardarPagoLicencia DB_RESPONSE: {Json}", LogJson.ToJson(pagoId));

                if (pagoId == 0)
                {
                    if (File.Exists(rutaAbsoluta)) File.Delete(rutaAbsoluta);
                    Log.Warning("LicenciaService.GuardarPagoLicencia VALIDACIÓN: {Mensaje}", "No se pudo registrar el comprobante.");
                    return new ModelResponse<ConsultorioPago> { IsSuccess = false, Message = "No se pudo registrar el comprobante." };
                }

                var creado = _dbWrapper.ObtenerPagoLicenciaById(pagoId, empresaId);
                var pago = (creado != null && creado.IsSuccess && creado.Response != null)
                    ? creado.Response
                    : new ConsultorioPago
                    {
                        Id = pagoId,
                        ConsultorioID = empresaId,
                        NombreArchivo = nombreOriginal,
                        ArchivoRuta = rutaRelativa,
                        Monto = monto,
                        EstatusPagoId = 1,
                        FechaCarga = DateTime.Now,
                        SubidoPor = usuario
                    };

                // Correo best-effort: un fallo de SMTP no revierte el registro.
                NotificarPagoRecibido(empresaId, usuario, pago);

                mr.IsSuccess = true;
                mr.Response = pago;
                mr.Message = "Comprobante recibido correctamente. Tu pago quedó pendiente de confirmación.";
                Log.Information("LicenciaService.GuardarPagoLicencia RESULTADO: IsSuccess=True, Message=Comprobante recibido correctamente., Id={Id}", pago.Id);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(rutaAbsoluta) && File.Exists(rutaAbsoluta))
                {
                    try { File.Delete(rutaAbsoluta); } catch { }
                }
                Log.Error(ex, "LicenciaService.GuardarPagoLicencia ERROR: {Message}", ex.Message);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al guardar el comprobante.";
            }
            return mr;
        }

        /// <summary>Devuelve los bytes del comprobante en disco para descarga.</summary>
        public byte[] ObtenerBytesPago(ConsultorioPago pago)
        {
            if (pago == null || string.IsNullOrWhiteSpace(pago.ArchivoRuta))
            {
                Log.Warning("LicenciaService.ObtenerBytesPago VALIDACIÓN: Comprobante sin ruta de archivo.");
                return null;
            }

            string rutaAbsoluta;
            if (pago.ArchivoRuta.StartsWith("/"))
                rutaAbsoluta = HostingEnvironment.MapPath("~" + pago.ArchivoRuta);
            else if (Path.IsPathRooted(pago.ArchivoRuta))
                rutaAbsoluta = pago.ArchivoRuta;
            else
                rutaAbsoluta = HostingEnvironment.MapPath("~/" + pago.ArchivoRuta);

            if (string.IsNullOrEmpty(rutaAbsoluta) || !File.Exists(rutaAbsoluta))
            {
                Log.Warning("LicenciaService.ObtenerBytesPago VALIDACIÓN: No existe el archivo del comprobante en disco. Id={Id}", pago.Id);
                return null;
            }

            var bytes = File.ReadAllBytes(rutaAbsoluta);
            Log.Debug("LicenciaService.ObtenerBytesPago ARCHIVO: {Json}", LogJson.ToJson(new { pago.Id, Longitud = bytes?.Length }));
            return bytes;
        }

        /// <summary>
        /// Notifica (best-effort) la recepción de un comprobante al primer usuario del
        /// consultorio, al uploader y a los correos fijos DESI. EmailHelper.EnviarEmail
        /// LANZA en fallo; por eso todo queda envuelto en try/catch (el SMTP no revierte).
        /// </summary>
        private void NotificarPagoRecibido(int empresaId, string usuario, ConsultorioPago pago)
        {
            try
            {
                var destinatarios = new List<string>();

                var primerUsuario = _dbWrapper.ObtenerPrimerUsuarioConsultorio(empresaId);
                if (primerUsuario != null && primerUsuario.IsSuccess && primerUsuario.Response != null &&
                    !string.IsNullOrWhiteSpace(primerUsuario.Response.Email))
                    destinatarios.Add(primerUsuario.Response.Email);

                var uploader = _dbWrapper.ObtenerUsuarioPorUserName(usuario);
                if (uploader != null && !string.IsNullOrWhiteSpace(uploader.Email))
                    destinatarios.Add(uploader.Email);

                AgregarCorreo(destinatarios, ConfigurationManager.AppSettings["PagoCorreoComprobante"]);
                AgregarCorreo(destinatarios, ConfigurationManager.AppSettings["PagoCorreoAviso"]);

                destinatarios = destinatarios
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (destinatarios.Count == 0)
                {
                    Log.Warning("LicenciaService.NotificarPagoRecibido VALIDACIÓN: Sin destinatarios de correo.");
                    return;
                }

                var ruta = HostingEnvironment.MapPath("~/Template/Template_PagoRecibido.html");
                if (string.IsNullOrEmpty(ruta) || !File.Exists(ruta))
                {
                    Log.Warning("Plantilla de correo de pago no encontrada en {Ruta}", ruta);
                    return;
                }

                var plantilla = File.ReadAllText(ruta);

                var baseUri = ConfigurationManager.AppSettings["BaseUri"] ?? string.Empty;
                if (!baseUri.EndsWith("/"))
                    baseUri += "/";

                var cuerpo = plantilla.Replace("{{UrlLicencia}}", baseUri + "Licencia");

                EmailHelper.EnviarEmail(
                    destinatarios,
                    "Comprobante de pago recibido - Consultorio Médico",
                    cuerpo);

                Log.Information("LicenciaService.NotificarPagoRecibido RESULTADO: Correo enviado a {Destinatarios}", string.Join(", ", destinatarios));
            }
            catch (Exception ex)
            {
                // Best-effort: el pago ya quedó registrado; solo se informa del fallo.
                Log.Error(ex, "No se pudo notificar el comprobante de pago del consultorio {EmpresaId}", empresaId);
            }
        }

        private static void AgregarCorreo(List<string> lista, string correo)
        {
            if (!string.IsNullOrWhiteSpace(correo))
                lista.Add(correo.Trim());
        }

        private static decimal ObtenerMontoAnual()
        {
            decimal monto;
            if (!decimal.TryParse(ConfigurationManager.AppSettings["PagoMontoAnual"], out monto))
                monto = 1200m;
            return monto;
        }
    }
}
