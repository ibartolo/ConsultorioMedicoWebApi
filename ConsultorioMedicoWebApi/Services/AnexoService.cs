using System;
using System.Collections.Generic;
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
    public class AnexoService
    {
        private const string CarpetaAnexos = "Attachment";
        private const long MaxPdfBytes = 5L * 1024 * 1024;
        private const long MaxImagenBytes = 2L * 1024 * 1024;

        private static readonly string[] ExtensionesPermitidas = { ".pdf", ".png", ".jpg", ".jpeg" };

        private readonly DbWrapper _dbWrapper;

        public AnexoService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<Anexo>> ObtenerAnexosPorPaciente(long pacienteId, string usuario, int empresaId)
        {
            try
            {
                Log.Information("AnexoService.ObtenerAnexosPorPaciente ENTRADA: {Json}", LogJson.ToJson(new { pacienteId, usuario, empresaId }));

                Log.Debug("AnexoService.ObtenerAnexosPorPaciente DB_REQUEST: {Json}", LogJson.ToJson(new { pacienteId, usuario, empresaId }));
                var result = _dbWrapper.ObtenerAnexosPorPaciente(pacienteId, usuario, empresaId);
                Log.Debug("AnexoService.ObtenerAnexosPorPaciente DB_RESPONSE: {Json}", LogJson.ToJson(new { Count = result?.Response?.Count, result?.IsSuccess, result?.Message }));

                Log.Information("AnexoService.ObtenerAnexosPorPaciente RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Count={Count}", result?.IsSuccess, result?.Message, result?.Response?.Count);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AnexoService.ObtenerAnexosPorPaciente ERROR: {Message}", ex.Message);
                return new ModelResponse<List<Anexo>> { IsSuccess = false, Message = "Ocurrió un error al obtener los anexos." };
            }
        }

        public ModelResponse<Anexo> ObtenerAnexoPorId(long id, string usuario, int empresaId)
        {
            try
            {
                Log.Information("AnexoService.ObtenerAnexoPorId ENTRADA: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));

                Log.Debug("AnexoService.ObtenerAnexoPorId DB_REQUEST: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));
                var result = _dbWrapper.ObtenerAnexoPorId(id, usuario, empresaId);
                Log.Debug("AnexoService.ObtenerAnexoPorId DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("AnexoService.ObtenerAnexoPorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AnexoService.ObtenerAnexoPorId ERROR: {Message}", ex.Message);
                return new ModelResponse<Anexo> { IsSuccess = false, Message = "Ocurrió un error al obtener el anexo." };
            }
        }

        /// <summary>
        /// Guarda un archivo anexo en Attachment/{consultorioId}/{pacienteId}/[{consultaId}/] y registra la fila en BD.
        /// Reglas: PDF hasta 5 MB; PNG/JPG/JPEG hasta 2 MB. El archivo físico se nombra con GUID.
        /// El tenant (consultorioId) proviene del claim (empresaId), nunca del cliente.
        /// </summary>
        public ModelResponse<Anexo> GuardarAnexo(long pacienteId, long? consultaId, string comentarios, HttpFileCollection files, string usuario, int empresaId)
        {
            var mr = new ModelResponse<Anexo>();
            string rutaAbsoluta = null;
            try
            {
                Log.Information("AnexoService.GuardarAnexo ENTRADA: {Json}", LogJson.ToJson(new { pacienteId, consultaId, comentarios, usuario, empresaId, FileCount = files?.Count }));

                if (pacienteId <= 0)
                {
                    Log.Warning("AnexoService.GuardarAnexo VALIDACIÓN: {Mensaje}", "Seleccione el paciente.");
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "Seleccione el paciente." };
                }

                if (files == null || files.Count == 0)
                {
                    Log.Warning("AnexoService.GuardarAnexo VALIDACIÓN: {Mensaje}", "Debe seleccionar un archivo.");
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "Debe seleccionar un archivo." };
                }

                var file = files[0];
                if (file == null || file.ContentLength == 0)
                {
                    Log.Warning("AnexoService.GuardarAnexo VALIDACIÓN: {Mensaje}", "El archivo está vacío.");
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "El archivo está vacío." };
                }

                var nombreOriginal = Path.GetFileName(file.FileName);
                if (string.IsNullOrWhiteSpace(nombreOriginal))
                {
                    Log.Warning("AnexoService.GuardarAnexo VALIDACIÓN: {Mensaje}", "El nombre del archivo no es válido.");
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "El nombre del archivo no es válido." };
                }

                if (nombreOriginal.Length > 150)
                {
                    Log.Warning("AnexoService.GuardarAnexo VALIDACIÓN: {Mensaje}", "El nombre del archivo no puede exceder 150 caracteres.");
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "El nombre del archivo no puede exceder 150 caracteres." };
                }

                var extension = Path.GetExtension(nombreOriginal).ToLowerInvariant();
                if (Array.IndexOf(ExtensionesPermitidas, extension) < 0)
                {
                    Log.Warning("AnexoService.GuardarAnexo VALIDACIÓN: {Mensaje}", "Solo se permiten archivos PDF, PNG, JPG o JPEG.");
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "Solo se permiten archivos PDF, PNG, JPG o JPEG." };
                }

                var maxBytes = extension == ".pdf" ? MaxPdfBytes : MaxImagenBytes;
                if (file.ContentLength > maxBytes)
                {
                    var limite = extension == ".pdf" ? "5 MB" : "2 MB";
                    Log.Warning("AnexoService.GuardarAnexo VALIDACIÓN: {Mensaje}", "El archivo excede el tamaño máximo permitido (" + limite + ").");
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "El archivo excede el tamaño máximo permitido (" + limite + ")." };
                }

                var consultaValida = consultaId.HasValue && consultaId.Value > 0;
                if (consultaValida)
                {
                    Log.Debug("AnexoService.GuardarAnexo DB_REQUEST (consulta pertenece a paciente): {Json}", LogJson.ToJson(new { consultaId = consultaId.Value, pacienteId, empresaId }));
                    var pertenece = _dbWrapper.ConsultaPerteneceAPaciente(consultaId.Value, pacienteId, empresaId);
                    Log.Debug("AnexoService.GuardarAnexo DB_RESPONSE (consulta pertenece a paciente): {Json}", LogJson.ToJson(pertenece));
                    if (!pertenece)
                    {
                        Log.Warning("AnexoService.GuardarAnexo VALIDACIÓN: {Mensaje}", "La consulta seleccionada no pertenece al paciente.");
                        return new ModelResponse<Anexo> { IsSuccess = false, Message = "La consulta seleccionada no pertenece al paciente." };
                    }
                }

                var nombreFisico = Guid.NewGuid().ToString() + extension;
                // Ruta por tenant: /Attachment/{consultorioId}/{pacienteId}/[{consultaId}/]{guid}.ext
                var rutaRelativa = "/" + CarpetaAnexos + "/" + empresaId + "/" + pacienteId;
                if (consultaValida)
                    rutaRelativa += "/" + consultaId.Value;
                rutaRelativa += "/" + nombreFisico;

                rutaAbsoluta = HostingEnvironment.MapPath("~" + rutaRelativa);
                if (string.IsNullOrEmpty(rutaAbsoluta))
                {
                    Log.Warning("AnexoService.GuardarAnexo VALIDACIÓN: {Mensaje}", "No se pudo resolver la ruta de almacenamiento.");
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "No se pudo resolver la ruta de almacenamiento." };
                }

                var directorio = Path.GetDirectoryName(rutaAbsoluta);
                if (!Directory.Exists(directorio))
                    Directory.CreateDirectory(directorio);

                file.SaveAs(rutaAbsoluta);
                Log.Debug("AnexoService.GuardarAnexo ARCHIVO: {Json}", LogJson.ToJson(new { NombreOriginal = nombreOriginal, file.ContentLength, extension }));

                var anexo = new Anexo
                {
                    Estatus = true,
                    Fecha = DateTime.Now,
                    Comentarios = Truncar(comentarios, 1500),
                    PacienteID = (int)pacienteId,
                    Nombre = nombreOriginal,
                    Ruta = rutaRelativa,
                    ConsultaID = consultaValida ? (int?)consultaId.Value : null,
                    ConsultorioID = empresaId
                };

                Log.Debug("AnexoService.GuardarAnexo DB_REQUEST: {Json}", LogJson.ToJson(new { anexo, usuario, empresaId }));
                var nuevoId = _dbWrapper.GuardarAnexo(anexo, usuario, empresaId);
                Log.Debug("AnexoService.GuardarAnexo DB_RESPONSE: {Json}", LogJson.ToJson(nuevoId));

                if (nuevoId == 0)
                {
                    if (File.Exists(rutaAbsoluta)) File.Delete(rutaAbsoluta);
                    Log.Warning("AnexoService.GuardarAnexo VALIDACIÓN: {Mensaje}", "No se pudo registrar el anexo.");
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "No se pudo registrar el anexo." };
                }

                anexo.Id = nuevoId;
                mr.IsSuccess = true;
                mr.Response = anexo;
                mr.Message = "Anexo guardado correctamente.";
                Log.Information("AnexoService.GuardarAnexo RESULTADO: IsSuccess=True, Message=Anexo guardado correctamente., Id={Id}", anexo.Id);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(rutaAbsoluta) && File.Exists(rutaAbsoluta))
                {
                    try { File.Delete(rutaAbsoluta); } catch { }
                }
                Log.Error(ex, "AnexoService.GuardarAnexo ERROR: {Message}", ex.Message);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al guardar el anexo.";
            }
            return mr;
        }

        private static string Truncar(string valor, int maximo)
        {
            if (string.IsNullOrEmpty(valor)) return valor;
            return valor.Length <= maximo ? valor : valor.Substring(0, maximo);
        }

        /// <summary>Elimina el anexo de la base y su archivo físico.</summary>
        public ModelResponse<bool> EliminarAnexo(long id, string usuario, int empresaId)
        {
            var mr = new ModelResponse<bool>();
            try
            {
                Log.Information("AnexoService.EliminarAnexo ENTRADA: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));

                Log.Debug("AnexoService.EliminarAnexo DB_REQUEST (obtener anexo): {Json}", LogJson.ToJson(new { id, usuario, empresaId }));
                var anexoResponse = _dbWrapper.ObtenerAnexoPorId(id, usuario, empresaId);
                Log.Debug("AnexoService.EliminarAnexo DB_RESPONSE (obtener anexo): {Json}", LogJson.ToJson(anexoResponse));

                if (anexoResponse == null || !anexoResponse.IsSuccess || anexoResponse.Response == null)
                {
                    Log.Warning("AnexoService.EliminarAnexo VALIDACIÓN: {Mensaje}", "Anexo no encontrado.");
                    return new ModelResponse<bool> { IsSuccess = false, Message = "Anexo no encontrado." };
                }

                var anexo = anexoResponse.Response;

                Log.Debug("AnexoService.EliminarAnexo DB_REQUEST (eliminar anexo): {Json}", LogJson.ToJson(new { id, usuario, empresaId }));
                var eliminado = _dbWrapper.EliminarAnexo(id, usuario, empresaId);
                Log.Debug("AnexoService.EliminarAnexo DB_RESPONSE (eliminar anexo): {Json}", LogJson.ToJson(eliminado));

                if (!eliminado)
                {
                    Log.Warning("AnexoService.EliminarAnexo VALIDACIÓN: {Mensaje}", "No fue posible eliminar el anexo de la base de datos.");
                    return new ModelResponse<bool> { IsSuccess = false, Message = "No fue posible eliminar el anexo de la base de datos." };
                }

                var archivoEliminado = EliminarArchivoAnexo(anexo.Ruta);

                mr.IsSuccess = true;
                mr.Response = true;
                mr.Message = archivoEliminado
                    ? "Anexo eliminado correctamente."
                    : "Anexo eliminado de la base de datos, pero no se pudo borrar el archivo físico.";
                Log.Information("AnexoService.EliminarAnexo RESULTADO: IsSuccess=True, Message={Message}, Id={Id}", mr.Message, id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AnexoService.EliminarAnexo ERROR: {Message}", ex.Message);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al eliminar el anexo.";
            }
            return mr;
        }

        private bool EliminarArchivoAnexo(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta)) return true;

            string rutaAbsoluta;
            if (ruta.StartsWith("/"))
                rutaAbsoluta = HostingEnvironment.MapPath("~" + ruta);
            else if (Path.IsPathRooted(ruta))
                rutaAbsoluta = ruta;
            else
                rutaAbsoluta = HostingEnvironment.MapPath("~/" + ruta);

            if (string.IsNullOrEmpty(rutaAbsoluta) || !File.Exists(rutaAbsoluta)) return true;

            try
            {
                File.Delete(rutaAbsoluta);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "No se pudo borrar el archivo del anexo {Ruta}", ruta);
                return false;
            }
        }

        /// <summary>Devuelve los bytes del archivo en disco para descarga.</summary>
        public byte[] ObtenerBytesAnexo(Anexo anexo)
        {
            if (anexo == null || string.IsNullOrWhiteSpace(anexo.Ruta))
            {
                Log.Warning("AnexoService.ObtenerBytesAnexo VALIDACIÓN: Anexo sin ruta de archivo.");
                return null;
            }

            string rutaAbsoluta;
            if (anexo.Ruta.StartsWith("/"))
                rutaAbsoluta = HostingEnvironment.MapPath("~" + anexo.Ruta);
            else if (Path.IsPathRooted(anexo.Ruta))
                rutaAbsoluta = anexo.Ruta; // rutas antiguas del WinForms (ej. M:\P\...)
            else
                rutaAbsoluta = HostingEnvironment.MapPath("~/" + anexo.Ruta);

            if (string.IsNullOrEmpty(rutaAbsoluta) || !File.Exists(rutaAbsoluta))
            {
                Log.Warning("AnexoService.ObtenerBytesAnexo VALIDACIÓN: No existe el archivo del anexo en disco. Id={Id}", anexo.Id);
                return null;
            }

            var bytes = File.ReadAllBytes(rutaAbsoluta);
            Log.Debug("AnexoService.ObtenerBytesAnexo ARCHIVO: {Json}", LogJson.ToJson(new { anexo.Id, Longitud = bytes?.Length }));
            return bytes;
        }
    }
}
