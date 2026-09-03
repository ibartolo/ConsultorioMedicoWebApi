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
        /// Guarda un archivo anexo: lo escribe a disco y registra la fila en BD.
        /// </summary>
        public ModelResponse<Anexo> GuardarAnexo(long pacienteId, long? consultaId, string comentarios, HttpFileCollection files, string usuario)
        {
            var mr = new ModelResponse<Anexo>();
            try
            {
                if (pacienteId <= 0)
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "Seleccione el paciente." };

                if (files == null || files.Count == 0)
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "Debe seleccionar un archivo." };

                var file = files[0];
                if (file == null || file.ContentLength == 0)
                    return new ModelResponse<Anexo> { IsSuccess = false, Message = "El archivo está vacío." };

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var nombreFisico = Guid.NewGuid().ToString("N") + extension;
                var rutaRelativa = "Anexos/" + pacienteId + "/" + nombreFisico;

                var rutaAbsoluta = HostingEnvironment.MapPath("~/" + rutaRelativa);
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
                    Comentarios = comentarios,
                    PacienteID = (int)pacienteId,
                    Nombre = file.FileName,
                    Ruta = rutaRelativa,
                    ConsultaID = (int?)consultaId
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
                Log.Error(ex, "Error en GuardarAnexo");
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al guardar el anexo.";
            }
            return mr;
        }

        /// <summary>Devuelve los bytes del archivo en disco para descarga.</summary>
        public byte[] ObtenerBytesAnexo(Anexo anexo)
        {
            if (anexo == null || string.IsNullOrWhiteSpace(anexo.Ruta)) return null;
            var rutaAbsoluta = HostingEnvironment.MapPath("~/" + anexo.Ruta);
            if (string.IsNullOrEmpty(rutaAbsoluta) || !File.Exists(rutaAbsoluta)) return null;
            return File.ReadAllBytes(rutaAbsoluta);
        }
    }
}
