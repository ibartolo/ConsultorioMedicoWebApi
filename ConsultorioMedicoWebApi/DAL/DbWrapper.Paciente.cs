using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using Serilog;

namespace ConsultorioMedicoWebApi.DAL
{
    public partial class DbWrapper
    {
        private PacienteDTO LlenarPacienteDTO(IDataReader r)
        {
            return new PacienteDTO
            {
                Id = Convert.ToInt32(r["Id"]),
                Nombre = r["Nombre"] as string,
                ApellidoPaterno = r["ApellidoPaterno"] as string,
                ApellidoMaterno = r["ApellidoMaterno"] as string,
                NombreCompleto = r["NombreCompleto"] as string,
                Edad = Convert.IsDBNull(r["Edad"]) ? (int?)null : Convert.ToInt32(r["Edad"]),
                Sexo = r["Sexo"] as string,
                Email = r["Email"] as string,
                FechaAlta = Convert.IsDBNull(r["FechaAlta"]) ? (DateTime?)null : Convert.ToDateTime(r["FechaAlta"]),
                TelefonoCel = r["TelefonoCel"] as string,
                Estatus = Convert.IsDBNull(r["Estatus"]) ? false : Convert.ToBoolean(r["Estatus"]),
                Comentarios = r["Comentarios"] as string,
                FechaNacimiento = Convert.IsDBNull(r["FechaNacimiento"]) ? (DateTime?)null : Convert.ToDateTime(r["FechaNacimiento"])
            };
        }

        private Paciente LlenarPaciente(IDataReader r)
        {
            return new Paciente
            {
                Id = Convert.ToInt32(r["Id"]),
                Nombre = r["Nombre"] as string,
                ApellidoPaterno = r["ApellidoPaterno"] as string,
                ApellidoMaterno = r["ApellidoMaterno"] as string,
                Email = r["Email"] as string,
                FechaAlta = Convert.IsDBNull(r["FechaAlta"]) ? (DateTime?)null : Convert.ToDateTime(r["FechaAlta"]),
                TelefonoCel = r["TelefonoCel"] as string,
                Comentarios = r["Comentarios"] as string,
                Estatus = Convert.IsDBNull(r["Estatus"]) ? false : Convert.ToBoolean(r["Estatus"]),
                Edad = Convert.IsDBNull(r["Edad"]) ? (int?)null : Convert.ToInt32(r["Edad"]),
                Sexo = r["Sexo"] as string,
                FechaNacimiento = Convert.IsDBNull(r["FechaNacimiento"]) ? (DateTime?)null : Convert.ToDateTime(r["FechaNacimiento"])
            };
        }

        /// <summary>Lista pacientes activos (Estatus=1). Reutiliza el SP existente GetAllPacienteByEstatus.</summary>
        public ModelResponse<List<PacienteDTO>> ObtenerPacientes(string usuario)
        {
            var mr = new ModelResponse<List<PacienteDTO>>();
            try
            {
                var pacientes = GetObjects("GetAllPacienteByEstatus", LlenarPacienteDTO, CommandType.StoredProcedure);
                mr.IsSuccess = true;
                mr.Response = pacientes.ToList();
                mr.Message = "Pacientes obtenidos correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener pacientes para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener los pacientes.";
            }
            return mr;
        }

        /// <summary>Obtiene un paciente por Id. SP nuevo: sp_nh_GetPacienteById.</summary>
        public ModelResponse<Paciente> ObtenerPacientePorId(long id, string usuario)
        {
            var mr = new ModelResponse<Paciente>();
            try
            {
                var paciente = GetObject("sp_nh_GetPacienteById", LlenarPaciente, CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Id", id) });

                if (paciente == null)
                {
                    mr.IsSuccess = false;
                    mr.Message = "Paciente no encontrado.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = paciente;
                mr.Message = "Paciente obtenido correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener paciente {Id}", id);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener el paciente.";
            }
            return mr;
        }

        /// <summary>Guarda o actualiza un paciente (upsert). SP nuevo: sp_nh_SavePaciente. Devuelve el Id.</summary>
        public ModelResponse<Paciente> GuardarOActualizarPaciente(Paciente p, string usuario)
        {
            var mr = new ModelResponse<Paciente>();
            try
            {
                var pars = new List<SqlParameter>
                {
                    new SqlParameter("@Id", p.Id),
                    new SqlParameter("@Nombre", (object)p.Nombre ?? DBNull.Value),
                    new SqlParameter("@ApellidoPaterno", (object)p.ApellidoPaterno ?? DBNull.Value),
                    new SqlParameter("@ApellidoMaterno", (object)p.ApellidoMaterno ?? DBNull.Value),
                    new SqlParameter("@Email", (object)p.Email ?? DBNull.Value),
                    new SqlParameter("@FechaAlta", (object)p.FechaAlta ?? DBNull.Value),
                    new SqlParameter("@TelefonoCel", (object)p.TelefonoCel ?? DBNull.Value),
                    new SqlParameter("@Comentarios", (object)p.Comentarios ?? DBNull.Value),
                    new SqlParameter("@Estatus", p.Estatus),
                    new SqlParameter("@Edad", (object)p.Edad ?? DBNull.Value),
                    new SqlParameter("@Sexo", (object)p.Sexo ?? DBNull.Value),
                    new SqlParameter("@FechaNacimiento", (object)p.FechaNacimiento ?? DBNull.Value)
                };

                var id = ExecuteScalar("sp_nh_SavePaciente", CommandType.StoredProcedure, pars);
                if (id == null || Convert.ToInt64(id) == 0)
                {
                    mr.IsSuccess = false;
                    mr.Message = "No fue posible guardar el paciente.";
                    return mr;
                }

                p.Id = Convert.ToInt32(id);
                mr.IsSuccess = true;
                mr.Response = p;
                mr.Message = "Paciente guardado correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar paciente para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al guardar el paciente.";
            }
            return mr;
        }

        /// <summary>Baja lógica de un paciente (Estatus=0). SP nuevo: sp_nh_DeletePaciente.</summary>
        public ModelResponse<bool> EliminarPaciente(int id, string usuario)
        {
            var mr = new ModelResponse<bool>();
            try
            {
                var filas = ExecuteNonQuery("sp_nh_DeletePaciente", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Id", id) });

                if (filas <= 0)
                {
                    mr.IsSuccess = false;
                    mr.Message = "Paciente no encontrado.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = true;
                mr.Message = "Paciente eliminado correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar paciente {Id} para {Usuario}", id, usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al eliminar el paciente.";
            }
            return mr;
        }
    }
}
