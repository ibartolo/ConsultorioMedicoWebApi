using System;
using System.Collections.Generic;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using Serilog;

namespace ConsultorioMedicoWebApi.Services
{
    public class TipoConsultaService
    {
        private readonly DbWrapper _dbWrapper;

        public TipoConsultaService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<TipoConsulta>> ObtenerTiposConsulta(string usuario, int empresaId)
        {
            try
            {
                return _dbWrapper.ObtenerTiposConsulta(usuario, empresaId, soloActivos: true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerTiposConsulta");
                return new ModelResponse<List<TipoConsulta>> { IsSuccess = false, Message = "Ocurrió un error al obtener los tipos de consulta." };
            }
        }

        public ModelResponse<List<TipoConsulta>> ObtenerTiposConsultaAdmin(string usuario, int empresaId)
        {
            try
            {
                return _dbWrapper.ObtenerTiposConsulta(usuario, empresaId, soloActivos: false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerTiposConsultaAdmin");
                return new ModelResponse<List<TipoConsulta>> { IsSuccess = false, Message = "Ocurrió un error al obtener los tipos de consulta." };
            }
        }

        public ModelResponse<TipoConsulta> ObtenerTipoConsultaPorId(long id, string usuario, int empresaId)
        {
            try
            {
                return _dbWrapper.ObtenerTipoConsultaPorId(id, usuario, empresaId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerTipoConsultaPorId");
                return new ModelResponse<TipoConsulta> { IsSuccess = false, Message = "Ocurrió un error al obtener el tipo de consulta." };
            }
        }

        public ModelResponse<TipoConsulta> GuardarOActualizarTipoConsulta(TipoConsulta tipoConsulta, string usuario, int empresaId)
        {
            try
            {
                var error = TipoConsultaValidator.Validar(tipoConsulta);
                if (error != null)
                    return new ModelResponse<TipoConsulta> { IsSuccess = false, Message = error };

                if (tipoConsulta.Id == 0)
                    tipoConsulta.Estatus = true;

                // El tenant SIEMPRE se asigna desde el claim (nunca del cliente).
                tipoConsulta.ConsultorioID = empresaId;

                return _dbWrapper.GuardarOActualizarTipoConsulta(tipoConsulta, usuario, empresaId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en GuardarOActualizarTipoConsulta");
                return new ModelResponse<TipoConsulta> { IsSuccess = false, Message = "Ocurrió un error al guardar el tipo de consulta." };
            }
        }

        public ModelResponse<bool> EliminarTipoConsulta(int id, string usuario, int empresaId)
        {
            try
            {
                if (id <= 0)
                    return new ModelResponse<bool> { IsSuccess = false, Message = "El identificador del tipo de consulta es inválido." };

                return _dbWrapper.EliminarTipoConsulta(id, usuario, empresaId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EliminarTipoConsulta");
                return new ModelResponse<bool> { IsSuccess = false, Message = "Ocurrió un error al eliminar el tipo de consulta." };
            }
        }
    }
}
