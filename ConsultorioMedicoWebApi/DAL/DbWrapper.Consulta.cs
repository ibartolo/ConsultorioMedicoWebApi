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
        private ConsultaDTO LlenarConsultaDTO(IDataReader r)
        {
            return new ConsultaDTO
            {
                Id = Convert.ToInt32(r["Id"]),
                Estatus = Convert.ToBoolean(r["Estatus"]),
                FechaConsulta = Convert.IsDBNull(r["FechaConsulta"]) ? (DateTime?)null : Convert.ToDateTime(r["FechaConsulta"]),
                PacienteID = Convert.ToInt32(r["PacienteID"]),
                TratamientoID = Convert.IsDBNull(r["TratamientoID"]) ? (int?)null : Convert.ToInt32(r["TratamientoID"]),
                CitaID = Convert.IsDBNull(r["CitaID"]) ? (int?)null : Convert.ToInt32(r["CitaID"]),
                Descripcion = r["Descripcion"] as string,
                TipoConsultaID = Convert.IsDBNull(r["TipoConsultaID"]) ? (int?)null : Convert.ToInt32(r["TipoConsultaID"]),
                DescripcionReceta = r["DescripcionReceta"] as string,
                Comentarios = r["Comentarios"] as string,
                FechaProximaConsulta = Convert.IsDBNull(r["FechaProximaConsulta"]) ? (DateTime?)null : Convert.ToDateTime(r["FechaProximaConsulta"]),
                PacienteNombre = r["PacienteNombre"] as string,
                TipoConsultaNombre = r["TipoConsultaNombre"] as string
            };
        }

        private Consulta LlenarConsulta(IDataReader r)
        {
            return new Consulta
            {
                Id = Convert.ToInt32(r["Id"]),
                Estatus = Convert.ToBoolean(r["Estatus"]),
                FechaConsulta = Convert.IsDBNull(r["FechaConsulta"]) ? (DateTime?)null : Convert.ToDateTime(r["FechaConsulta"]),
                PacienteID = Convert.ToInt32(r["PacienteID"]),
                TratamientoID = Convert.IsDBNull(r["TratamientoID"]) ? (int?)null : Convert.ToInt32(r["TratamientoID"]),
                CitaID = Convert.IsDBNull(r["CitaID"]) ? (int?)null : Convert.ToInt32(r["CitaID"]),
                Descripcion = r["Descripcion"] as string,
                TipoConsultaID = Convert.IsDBNull(r["TipoConsultaID"]) ? (int?)null : Convert.ToInt32(r["TipoConsultaID"]),
                DescripcionReceta = r["DescripcionReceta"] as string,
                Comentarios = r["Comentarios"] as string,
                FechaProximaConsulta = Convert.IsDBNull(r["FechaProximaConsulta"]) ? (DateTime?)null : Convert.ToDateTime(r["FechaProximaConsulta"])
            };
        }

        /// <summary>Lista consultas activas. SP nuevo: sp_nh_GetAllConsultas.</summary>
        public ModelResponse<List<ConsultaDTO>> ObtenerConsultas(string usuario)
        {
            var mr = new ModelResponse<List<ConsultaDTO>>();
            try
            {
                var consultas = GetObjects("sp_nh_GetAllConsultas", LlenarConsultaDTO, CommandType.StoredProcedure);
                mr.IsSuccess = true;
                mr.Response = consultas.ToList();
                mr.Message = "Consultas obtenidas correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener consultas para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener las consultas.";
            }
            return mr;
        }

        /// <summary>Obtiene una consulta por Id. SP nuevo: sp_nh_GetConsultaById.</summary>
        public ModelResponse<Consulta> ObtenerConsultaPorId(long id, string usuario)
        {
            var mr = new ModelResponse<Consulta>();
            try
            {
                var consulta = GetObject("sp_nh_GetConsultaById", LlenarConsulta, CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Id", id) });

                if (consulta == null)
                {
                    mr.IsSuccess = false;
                    mr.Message = "Consulta no encontrada.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = consulta;
                mr.Message = "Consulta obtenida correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener consulta {Id}", id);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener la consulta.";
            }
            return mr;
        }

        /// <summary>Guarda o actualiza una consulta (upsert). SP nuevo: sp_nh_SaveConsulta. Devuelve el Id.</summary>
        public ModelResponse<Consulta> GuardarOActualizarConsulta(Consulta c, string usuario)
        {
            var mr = new ModelResponse<Consulta>();
            try
            {
                var pars = new List<SqlParameter>
                {
                    new SqlParameter("@Id", c.Id),
                    new SqlParameter("@Estatus", c.Estatus),
                    new SqlParameter("@FechaConsulta", (object)c.FechaConsulta ?? DBNull.Value),
                    new SqlParameter("@PacienteID", c.PacienteID),
                    new SqlParameter("@TratamientoID", (object)c.TratamientoID ?? DBNull.Value),
                    new SqlParameter("@CitaID", (object)c.CitaID ?? DBNull.Value),
                    new SqlParameter("@Descripcion", (object)c.Descripcion ?? DBNull.Value),
                    new SqlParameter("@TipoConsultaID", (object)c.TipoConsultaID ?? DBNull.Value),
                    new SqlParameter("@DescripcionReceta", (object)c.DescripcionReceta ?? DBNull.Value),
                    new SqlParameter("@Comentarios", (object)c.Comentarios ?? DBNull.Value),
                    new SqlParameter("@FechaProximaConsulta", (object)c.FechaProximaConsulta ?? DBNull.Value)
                };

                var id = ExecuteScalar("sp_nh_SaveConsulta", CommandType.StoredProcedure, pars);
                if (id == null || Convert.ToInt64(id) == 0)
                {
                    mr.IsSuccess = false;
                    mr.Message = "No fue posible guardar la consulta.";
                    return mr;
                }

                c.Id = Convert.ToInt32(id);
                mr.IsSuccess = true;
                mr.Response = c;
                mr.Message = "Consulta guardada correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar consulta para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al guardar la consulta.";
            }
            return mr;
        }

        /// <summary>Obtiene la receta de una consulta. Reutiliza el SP existente GetRecetaByConsulta.</summary>
        public ModelResponse<RecetaDTO> ObtenerRecetaPorConsulta(long consultaId, string usuario)
        {
            var mr = new ModelResponse<RecetaDTO>();
            try
            {
                var receta = GetObject("GetRecetaByConsulta",
                    new Func<IDataReader, RecetaDTO>(r => new RecetaDTO
                    {
                        Paciente = r["Paciente"] as string,
                        FechaConsulta = Convert.IsDBNull(r["FechaConsulta"]) ? (DateTime?)null : Convert.ToDateTime(r["FechaConsulta"]),
                        Consulta = r["Consulta"] as string,
                        Medicamento = r["Medicamento"] as string,
                        TipoConsulta = r["TipoConsulta"] as string,
                        ProximaConsulta = Convert.IsDBNull(r["ProximaConsulta"]) ? (DateTime?)null : Convert.ToDateTime(r["ProximaConsulta"])
                    }),
                    CommandType.StoredProcedure,
                    new[] { new SqlParameter("@consultaID", consultaId) });

                if (receta == null)
                {
                    mr.IsSuccess = false;
                    mr.Message = "Receta no encontrada.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = receta;
                mr.Message = "Receta obtenida correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener receta de consulta {Id}", consultaId);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener la receta.";
            }
            return mr;
        }

        /// <summary>Lista consultas por paciente. Reutiliza el SP existente GetAllConsultaByPacienteID.</summary>
        public ModelResponse<List<ConsultaDTO>> ObtenerConsultasPorPaciente(long pacienteId, string usuario)
        {
            var mr = new ModelResponse<List<ConsultaDTO>>();
            try
            {
                var consultas = GetObjects("GetAllConsultaByPacienteID",
                    new Func<IDataReader, ConsultaDTO>(r => new ConsultaDTO
                    {
                        Id = Convert.ToInt32(r["Id"]),
                        Estatus = Convert.ToBoolean(r["Estatus"]),
                        FechaConsulta = Convert.IsDBNull(r["FechaConsulta"]) ? (DateTime?)null : Convert.ToDateTime(r["FechaConsulta"]),
                        Descripcion = r["Descripcion"] as string,
                        PacienteID = Convert.ToInt32(r["PacienteID"]),
                        TratamientoID = Convert.IsDBNull(r["TratamientoID"]) ? (int?)null : Convert.ToInt32(r["TratamientoID"]),
                        CitaID = Convert.IsDBNull(r["CitaID"]) ? (int?)null : Convert.ToInt32(r["CitaID"]),
                        TipoConsultaID = Convert.IsDBNull(r["TipoConsultaID"]) ? (int?)null : Convert.ToInt32(r["TipoConsultaID"]),
                        TipoConsultaNombre = r["Nombre"] as string,
                        DescripcionReceta = r["DescripcionReceta"] as string
                    }),
                    CommandType.StoredProcedure,
                    new[] { new SqlParameter("@IdPaciente", pacienteId) });

                mr.IsSuccess = true;
                mr.Response = consultas.ToList();
                mr.Message = "Consultas obtenidas correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener consultas del paciente {Id}", pacienteId);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener las consultas.";
            }
            return mr;
        }
    }
}
