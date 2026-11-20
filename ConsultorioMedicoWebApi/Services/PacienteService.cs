using System;
using System.Collections.Generic;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoWebApi.DAL;
using ConsultorioMedicoWebApi.Helpers;
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
                Log.Information("PacienteService.ObtenerPacientes ENTRADA: {Json}", LogJson.ToJson(new { usuario, empresaId }));

                Log.Debug("PacienteService.ObtenerPacientes DB_REQUEST: {Json}", LogJson.ToJson(new { usuario, empresaId }));
                var result = _dbWrapper.ObtenerPacientes(usuario, empresaId);
                Log.Debug("PacienteService.ObtenerPacientes DB_RESPONSE: {Json}", LogJson.ToJson(new { Count = result?.Response?.Count, result?.IsSuccess, result?.Message }));

                Log.Information("PacienteService.ObtenerPacientes RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Count={Count}", result?.IsSuccess, result?.Message, result?.Response?.Count);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PacienteService.ObtenerPacientes ERROR: {Message}", ex.Message);
                return new ModelResponse<List<PacienteDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener los pacientes." };
            }
        }

        public ModelResponse<Paciente> ObtenerPacientePorId(long id, string usuario, int empresaId)
        {
            try
            {
                Log.Information("PacienteService.ObtenerPacientePorId ENTRADA: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));

                Log.Debug("PacienteService.ObtenerPacientePorId DB_REQUEST: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));
                var result = _dbWrapper.ObtenerPacientePorId(id, usuario, empresaId);
                Log.Debug("PacienteService.ObtenerPacientePorId DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("PacienteService.ObtenerPacientePorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PacienteService.ObtenerPacientePorId ERROR: {Message}", ex.Message);
                return new ModelResponse<Paciente> { IsSuccess = false, Message = "Ocurrió un error al obtener el paciente." };
            }
        }

        public ModelResponse<Paciente> GuardarOActualizarPaciente(Paciente paciente, string usuario, int empresaId)
        {
            try
            {
                Log.Information("PacienteService.GuardarOActualizarPaciente ENTRADA: {Json}", LogJson.ToJson(new { paciente, usuario, empresaId }));

                // 1. Validación de negocio (garantiza integridad, también para app móvil)
                var error = PacienteValidator.Validar(paciente);
                if (error != null)
                {
                    Log.Warning("PacienteService.GuardarOActualizarPaciente VALIDACIÓN: {Mensaje}", error);
                    return new ModelResponse<Paciente> { IsSuccess = false, Message = error };
                }

                // 2. Lógica de alta (fallback para clientes que no envían auditoría)
                if (paciente.Id == 0)
                {
                    paciente.Estatus = true;
                    if (paciente.FechaAlta == null) paciente.FechaAlta = DateTime.Now;
                }

                // 3. El tenant SIEMPRE se asigna desde el claim (nunca del cliente).
                paciente.ConsultorioID = empresaId;

                Log.Debug("PacienteService.GuardarOActualizarPaciente DB_REQUEST: {Json}", LogJson.ToJson(new { paciente, usuario, empresaId }));
                var result = _dbWrapper.GuardarOActualizarPaciente(paciente, usuario, empresaId);
                Log.Debug("PacienteService.GuardarOActualizarPaciente DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("PacienteService.GuardarOActualizarPaciente RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, result?.Response?.Id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PacienteService.GuardarOActualizarPaciente ERROR: {Message}", ex.Message);
                return new ModelResponse<Paciente> { IsSuccess = false, Message = "Ocurrió un error al guardar el paciente." };
            }
        }

        public ModelResponse<bool> EliminarPaciente(int id, string usuario, int empresaId)
        {
            try
            {
                Log.Information("PacienteService.EliminarPaciente ENTRADA: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));

                if (id <= 0)
                {
                    Log.Warning("PacienteService.EliminarPaciente VALIDACIÓN: {Mensaje}", "El identificador del paciente es inválido.");
                    return new ModelResponse<bool> { IsSuccess = false, Message = "El identificador del paciente es inválido." };
                }

                Log.Debug("PacienteService.EliminarPaciente DB_REQUEST: {Json}", LogJson.ToJson(new { id, usuario, empresaId }));
                var result = _dbWrapper.EliminarPaciente(id, usuario, empresaId);
                Log.Debug("PacienteService.EliminarPaciente DB_RESPONSE: {Json}", LogJson.ToJson(result));

                Log.Information("PacienteService.EliminarPaciente RESULTADO: IsSuccess={IsSuccess}, Message={Message}, Id={Id}", result?.IsSuccess, result?.Message, id);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PacienteService.EliminarPaciente ERROR: {Message}", ex.Message);
                return new ModelResponse<bool> { IsSuccess = false, Message = "Ocurrió un error al eliminar el paciente." };
            }
        }
    }
}
