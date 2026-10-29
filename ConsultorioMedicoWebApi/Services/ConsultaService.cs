using System;
using System.Collections.Generic;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using Serilog;

namespace ConsultorioMedicoWebApi.Services
{
    public class ConsultaService
    {
        private readonly DbWrapper _dbWrapper;

        public ConsultaService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<ConsultaDTO>> ObtenerConsultas(string usuario)
        {
            try
            {
                Log.Information("ConsultaService.ObtenerConsultas para {Usuario}", usuario);
                return _dbWrapper.ObtenerConsultas(usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerConsultas");
                return new ModelResponse<List<ConsultaDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener las consultas." };
            }
        }

        public ModelResponse<Consulta> ObtenerConsultaPorId(long id, string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerConsultaPorId(id, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerConsultaPorId");
                return new ModelResponse<Consulta> { IsSuccess = false, Message = "Ocurrió un error al obtener la consulta." };
            }
        }

        public ModelResponse<Consulta> GuardarOActualizarConsulta(Consulta consulta, string usuario)
        {
            try
            {
                // 1. Validación de negocio (garantiza integridad, también para app móvil)
                var error = ConsultaValidator.Validar(consulta);
                if (error != null)
                    return new ModelResponse<Consulta> { IsSuccess = false, Message = error };

                // 2. Lógica de alta
                if (consulta.Id == 0)
                    consulta.Estatus = true;

                return _dbWrapper.GuardarOActualizarConsulta(consulta, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en GuardarOActualizarConsulta");
                return new ModelResponse<Consulta> { IsSuccess = false, Message = "Ocurrió un error al guardar la consulta." };
            }
        }

        public ModelResponse<RecetaDTO> ObtenerRecetaPorConsulta(long consultaId, int institucionId, string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerRecetaPorConsulta(consultaId, institucionId, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerRecetaPorConsulta");
                return new ModelResponse<RecetaDTO> { IsSuccess = false, Message = "Ocurrió un error al obtener la receta." };
            }
        }

        public ModelResponse<List<ConsultaDTO>> ObtenerConsultasPorPaciente(long pacienteId, string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerConsultasPorPaciente(pacienteId, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerConsultasPorPaciente");
                return new ModelResponse<List<ConsultaDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener las consultas." };
            }
        }
    }
}
