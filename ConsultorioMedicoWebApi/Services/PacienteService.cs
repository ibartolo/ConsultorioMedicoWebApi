using System;
using System.Collections.Generic;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using Serilog;

namespace ConsultorioMedicoWebApi.Services
{
    public class PacienteService
    {
        private readonly DbWrapper _dbWrapper;

        public PacienteService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<PacienteDTO>> ObtenerPacientes(string usuario)
        {
            try
            {
                Log.Information("PacienteService.ObtenerPacientes para {Usuario}", usuario);
                return _dbWrapper.ObtenerPacientes(usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerPacientes");
                return new ModelResponse<List<PacienteDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener los pacientes." };
            }
        }

        public ModelResponse<Paciente> ObtenerPacientePorId(long id, string usuario)
        {
            try
            {
                return _dbWrapper.ObtenerPacientePorId(id, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerPacientePorId");
                return new ModelResponse<Paciente> { IsSuccess = false, Message = "Ocurrió un error al obtener el paciente." };
            }
        }

        public ModelResponse<Paciente> GuardarOActualizarPaciente(Paciente paciente, string usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(paciente.Nombre))
                    return new ModelResponse<Paciente> { IsSuccess = false, Message = "El nombre es requerido." };

                return _dbWrapper.GuardarOActualizarPaciente(paciente, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en GuardarOActualizarPaciente");
                return new ModelResponse<Paciente> { IsSuccess = false, Message = "Ocurrió un error al guardar el paciente." };
            }
        }
    }
}
