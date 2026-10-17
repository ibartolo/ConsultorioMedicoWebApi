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
        private CitaDTO LlenarCitaDTO(IDataReader r)
        {
            return new CitaDTO
            {
                Id = Convert.ToInt32(r["Id"]),
                Estatus = Convert.ToBoolean(r["Estatus"]),
                FechaCita = Convert.IsDBNull(r["FechaCita"]) ? (DateTime?)null : Convert.ToDateTime(r["FechaCita"]),
                PacienteID = Convert.ToInt32(r["PacienteID"]),
                Comentarios = r["Comentarios"] as string,
                TratamientoID = Convert.IsDBNull(r["TratamientoID"]) ? (int?)null : Convert.ToInt32(r["TratamientoID"]),
                Estado = Convert.IsDBNull(r["Estado"]) ? (int?)null : Convert.ToInt32(r["Estado"]),
                PacienteNombre = r["PacienteNombre"] as string,
                TratamientoNombre = r["TratamientoNombre"] as string
            };
        }

        private Cita LlenarCita(IDataReader r)
        {
            return new Cita
            {
                Id = Convert.ToInt32(r["Id"]),
                Estatus = Convert.ToBoolean(r["Estatus"]),
                FechaCita = Convert.IsDBNull(r["FechaCita"]) ? (DateTime?)null : Convert.ToDateTime(r["FechaCita"]),
                PacienteID = Convert.ToInt32(r["PacienteID"]),
                Comentarios = r["Comentarios"] as string,
                TratamientoID = Convert.IsDBNull(r["TratamientoID"]) ? (int?)null : Convert.ToInt32(r["TratamientoID"]),
                Estado = Convert.IsDBNull(r["Estado"]) ? (int?)null : Convert.ToInt32(r["Estado"])
            };
        }

        /// <summary>Lista citas activas. SP nuevo: sp_nh_GetAllCitas.</summary>
        public ModelResponse<List<CitaDTO>> ObtenerCitas(string usuario)
        {
            var mr = new ModelResponse<List<CitaDTO>>();
            try
            {
                var citas = GetObjects("sp_nh_GetAllCitas", LlenarCitaDTO, CommandType.StoredProcedure);
                mr.IsSuccess = true;
                mr.Response = citas.ToList();
                mr.Message = "Citas obtenidas correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener citas para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener las citas.";
            }
            return mr;
        }

        /// <summary>Obtiene una cita por Id. SP nuevo: sp_nh_GetCitaById.</summary>
        public ModelResponse<Cita> ObtenerCitaPorId(long id, string usuario)
        {
            var mr = new ModelResponse<Cita>();
            try
            {
                var cita = GetObject("sp_nh_GetCitaById", LlenarCita, CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Id", id) });

                if (cita == null)
                {
                    mr.IsSuccess = false;
                    mr.Message = "Cita no encontrada.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = cita;
                mr.Message = "Cita obtenida correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener cita {Id}", id);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener la cita.";
            }
            return mr;
        }

        /// <summary>Guarda o actualiza una cita (upsert). SP nuevo: sp_nh_SaveCita. Devuelve el Id.</summary>
        public ModelResponse<Cita> GuardarOActualizarCita(Cita c, string usuario)
        {
            var mr = new ModelResponse<Cita>();
            try
            {
                var pars = new List<SqlParameter>
                {
                    new SqlParameter("@Id", c.Id),
                    new SqlParameter("@Estatus", c.Estatus),
                    new SqlParameter("@FechaCita", (object)c.FechaCita ?? DBNull.Value),
                    new SqlParameter("@PacienteID", c.PacienteID),
                    new SqlParameter("@Comentarios", (object)c.Comentarios ?? DBNull.Value),
                    new SqlParameter("@TratamientoID", (object)c.TratamientoID ?? DBNull.Value),
                    new SqlParameter("@Estado", (object)c.Estado ?? DBNull.Value)
                };

                var id = ExecuteScalar("sp_nh_SaveCita", CommandType.StoredProcedure, pars);
                if (id == null || Convert.ToInt64(id) == 0)
                {
                    mr.IsSuccess = false;
                    mr.Message = "No fue posible guardar la cita.";
                    return mr;
                }

                c.Id = Convert.ToInt32(id);
                mr.IsSuccess = true;
                mr.Response = c;
                mr.Message = "Cita guardada correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar cita para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al guardar la cita.";
            }
            return mr;
        }

        /// <summary>Baja lógica de una cita (Estatus=0). SP nuevo: sp_nh_DeleteCita.</summary>
        public ModelResponse<bool> EliminarCita(int id, string usuario)
        {
            var mr = new ModelResponse<bool>();
            try
            {
                var filas = ExecuteNonQuery("sp_nh_DeleteCita", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Id", id) });

                if (filas <= 0)
                {
                    mr.IsSuccess = false;
                    mr.Message = "Cita no encontrada.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = true;
                mr.Message = "Cita eliminada correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar cita {Id} para {Usuario}", id, usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al eliminar la cita.";
            }
            return mr;
        }

        /// <summary>Lista citas por paciente. Reutiliza el SP existente GetAllCitaByPacienteID.</summary>
        public ModelResponse<List<CitaDTO>> ObtenerCitasPorPaciente(long pacienteId, string usuario)
        {
            var mr = new ModelResponse<List<CitaDTO>>();
            try
            {
                var citas = GetObjects("GetAllCitaByPacienteID",
                    new Func<IDataReader, CitaDTO>(r => new CitaDTO
                    {
                        Id = Convert.ToInt32(r["Id"]),
                        FechaCita = Convert.IsDBNull(r["FechaCita"]) ? (DateTime?)null : Convert.ToDateTime(r["FechaCita"]),
                        Comentarios = r["Comentarios"] as string,
                        TratamientoNombre = r["Nombre"] as string,
                        Estatus = Convert.ToBoolean(r["Estatus"]),
                        PacienteID = Convert.ToInt32(r["PacienteID"]),
                        TratamientoID = Convert.IsDBNull(r["TratamientoID"]) ? (int?)null : Convert.ToInt32(r["TratamientoID"]),
                        Estado = Convert.IsDBNull(r["Estado"]) ? (int?)null : Convert.ToInt32(r["Estado"])
                    }),
                    CommandType.StoredProcedure,
                    new[] { new SqlParameter("@IdPaciente", pacienteId) });

                mr.IsSuccess = true;
                mr.Response = citas.ToList();
                mr.Message = "Citas obtenidas correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener citas del paciente {Id}", pacienteId);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener las citas.";
            }
            return mr;
        }
    }
}
