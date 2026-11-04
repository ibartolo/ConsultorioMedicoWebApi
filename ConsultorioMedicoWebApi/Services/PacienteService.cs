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

        public ModelResponse<List<PacienteDTO>> ObtenerPacientes(string usuario, int empresaId)
        {
            try
            {
                Log.Information("PacienteService.ObtenerPacientes para {Usuario} (tenant {EmpresaId})", usuario, empresaId);
                return _dbWrapper.ObtenerPacientes(usuario, empresaId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerPacientes");
                return new ModelResponse<List<PacienteDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener los pacientes." };
            }
        }

        public ModelResponse<Paciente> ObtenerPacientePorId(long id, string usuario, int empresaId)
        {
            try
            {
                return _dbWrapper.ObtenerPacientePorId(id, usuario, empresaId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerPacientePorId");
                return new ModelResponse<Paciente> { IsSuccess = false, Message = "Ocurrió un error al obtener el paciente." };
            }
        }

        public ModelResponse<Paciente> GuardarOActualizarPaciente(Paciente paciente, string usuario, int empresaId)
        {
            try
            {
                // 1. Validación de negocio (garantiza integridad, también para app móvil)
                var error = PacienteValidator.Validar(paciente);
                if (error != null)
                    return new ModelResponse<Paciente> { IsSuccess = false, Message = error };

                // 2. Lógica de alta (fallback para clientes que no envían auditoría)
                if (paciente.Id == 0)
                {
                    paciente.Estatus = true;
                    if (paciente.FechaAlta == null) paciente.FechaAlta = DateTime.Now;
                }

                // 3. El tenant SIEMPRE se asigna desde el claim (nunca del cliente).
                paciente.ConsultorioID = empresaId;

                return _dbWrapper.GuardarOActualizarPaciente(paciente, usuario, empresaId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en GuardarOActualizarPaciente");
                return new ModelResponse<Paciente> { IsSuccess = false, Message = "Ocurrió un error al guardar el paciente." };
            }
        }

        public ModelResponse<bool> EliminarPaciente(int id, string usuario, int empresaId)
        {
            try
            {
                if (id <= 0)
                    return new ModelResponse<bool> { IsSuccess = false, Message = "El identificador del paciente es inválido." };

                return _dbWrapper.EliminarPaciente(id, usuario, empresaId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EliminarPaciente");
                return new ModelResponse<bool> { IsSuccess = false, Message = "Ocurrió un error al eliminar el paciente." };
            }
        }
    }
}
