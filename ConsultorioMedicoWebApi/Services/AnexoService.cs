using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Hosting;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
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

        public ModelResponse<List<Anexo>> ObtenerAnexosPorPaciente(long pacienteId, string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerAnexosPorPaciente(pacienteId, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerAnexosPorPaciente");
                return new ModelResponse<List<Anexo>> { IsSuccess = false, Message = "Ocurrió un error al obtener los anexos." };
            }
        }

        public ModelResponse<Anexo> ObtenerAnexoPorId(long id, string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerAnexoPorId(id, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerAnexoPorId");
                return new ModelResponse<Anexo> { IsSuccess = false, Message = "Ocurrió un error al obtener el anexo." };
            }
        }

        /// <summary>
        /// Guarda un archivo anexo en Attachment/{pacienteId}/[{consultaId}/] y registra la fila en BD.
        /// Reglas: PDF hasta 5 MB; PNG/JPG/JPEG hasta 2 MB. El archivo físico se nombra con GUID.
        /// </summary>
        public ModelResponse<Anexo> GuardarAnexo(long pacienteId, long? consultaId, string comentarios, HttpFileCollection files, string usuario)
        {
            var mr = new ModelResponse<Anexo>();
            string rutaAbsoluta = null;
            try
            {
                if (pacienteId <= 0)
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "Seleccione el paciente." };

                if (files == null || files.Count == 0)
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "Debe seleccionar un archivo." };

                var file = files[0];
                if (file == null || file.ContentLength == 0)
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "El archivo está vacío." };

                var nombreOriginal = Path.GetFileName(file.FileName);
                if (string.IsNullOrWhiteSpace(nombreOriginal))
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "El nombre del archivo no es válido." };

                if (nombreOriginal.Length > 150)
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "El nombre del archivo no puede exceder 150 caracteres." };

                var extension = Path.GetExtension(nombreOriginal).ToLowerInvariant();
                if (Array.IndexOf(ExtensionesPermitidas, extension) < 0)
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "Solo se permiten archivos PDF, PNG, JPG o JPEG." };

                var maxBytes = extension == ".pdf" ? MaxPdfBytes : MaxImagenBytes;
                if (file.ContentLength > maxBytes)
                {
                    var limite = extension == ".pdf" ? "5 MB" : "2 MB";
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "El archivo excede el tamaño máximo permitido (" + limite + ")." };
                }

                var consultaValida = consultaId.HasValue && consultaId.Value > 0;
                if (consultaValida && !_dbWrapper.ConsultaPerteneceAPaciente(consultaId.Value, pacienteId))
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "La consulta seleccionada no pertenece al paciente." };

                var nombreFisico = Guid.NewGuid().ToString() + extension;
                var rutaRelativa = "/" + CarpetaAnexos + "/" + pacienteId;
                if (consultaValida)
                    rutaRelativa += "/" + consultaId.Value;
                rutaRelativa += "/" + nombreFisico;

                rutaAbsoluta = HostingEnvironment.MapPath("~" + rutaRelativa);
                if (string.IsNullOrEmpty(rutaAbsoluta))
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "No se pudo resolver la ruta de almacenamiento." };

                var directorio = Path.GetDirectoryName(rutaAbsoluta);
                if (!Directory.Exists(directorio))
                    Directory.CreateDirectory(directorio);

                file.SaveAs(rutaAbsoluta);

                var anexo = new Anexo
                {
                    Estatus = true,
                    Fecha = DateTime.Now,
                    Comentarios = Truncar(comentarios, 1500),
                    PacienteID = (int)pacienteId,
                    Nombre = nombreOriginal,
                    Ruta = rutaRelativa,
                    ConsultaID = consultaValida ? (int?)consultaId.Value : null
                };

                var nuevoId = _dbWrapper.GuardarAnexo(anexo, usuario);
                if (nuevoId == 0)
                {
                    if (File.Exists(rutaAbsoluta)) File.Delete(rutaAbsoluta);
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "No se pudo registrar el anexo." };
                }

                anexo.Id = nuevoId;
                mr.IsSuccess = true;
                mr.Response = anexo;
                mr.Message = "Anexo guardado correctamente.";
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(rutaAbsoluta) && File.Exists(rutaAbsoluta))
                {
                    try { File.Delete(rutaAbsoluta); } catch { }
                }
                Log.Error(ex, "Error en GuardarAnexo");
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

        /// <summary>Devuelve los bytes del archivo en disco para descarga.</summary>
        public byte[] ObtenerBytesAnexo(Anexo anexo)
        {
            if (anexo == null || string.IsNullOrWhiteSpace(anexo.Ruta)) return null;

            string rutaAbsoluta;
            if (anexo.Ruta.StartsWith("/"))
                rutaAbsoluta = HostingEnvironment.MapPath("~" + anexo.Ruta);
            else if (Path.IsPathRooted(anexo.Ruta))
                rutaAbsoluta = anexo.Ruta; // rutas antiguas del WinForms (ej. M:\P\...)
            else
                rutaAbsoluta = HostingEnvironment.MapPath("~/" + anexo.Ruta);

            if (string.IsNullOrEmpty(rutaAbsoluta) || !File.Exists(rutaAbsoluta)) return null;
            return File.ReadAllBytes(rutaAbsoluta);
        }
    }
}
