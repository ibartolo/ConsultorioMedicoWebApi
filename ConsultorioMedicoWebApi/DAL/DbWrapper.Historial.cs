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
        /// <summary>Lista el historial de tratamientos de un paciente (Cita + Consulta). SP nuevo: sp_nh_GetTratamientosPorPaciente.</summary>
        public ModelResponse<List<TratamientoHistoricoDTO>> ObtenerTratamientosPorPaciente(long pacienteId, string usuario)
        {
            var mr = new ModelResponse<List<TratamientoHistoricoDTO>>();
            try
            {
                var tratamientos = GetObjects("sp_nh_GetTratamientosPorPaciente",
                    new Func<IDataReader, TratamientoHistoricoDTO>(r => new TratamientoHistoricoDTO
                    {
                        Id = Convert.ToInt32(r["Id"]),
                        Fecha = Convert.IsDBNull(r["Fecha"]) ? (DateTime?)null : Convert.ToDateTime(r["Fecha"]),
                        TratamientoNombre = r["TratamientoNombre"] as string,
                        Origen = r["Origen"] as string,
                        Comentarios = r["Comentarios"] as string
                    }),
                    CommandType.StoredProcedure,
                    new[] { new SqlParameter("@IdPaciente", pacienteId) });

                mr.IsSuccess = true;
                mr.Response = tratamientos.ToList();
                mr.Message = "Tratamientos obtenidos correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener el historial de tratamientos del paciente {Id}", pacienteId);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener el historial de tratamientos.";
            }
            return mr;
        }

        /// <summary>Lista los paquetes adquiridos por un paciente. SP nuevo: sp_nh_GetPaquetesPorPaciente.</summary>
        public ModelResponse<List<PacientePaqueteLecturaDTO>> ObtenerPaquetesPorPaciente(int pacienteId, string usuario)
        {
            var mr = new ModelResponse<List<PacientePaqueteLecturaDTO>>();
            try
            {
                var paquetes = GetObjects("sp_nh_GetPaquetesPorPaciente",
                    new Func<IDataReader, PacientePaqueteLecturaDTO>(r => new PacientePaqueteLecturaDTO
                    {
                        Id = Convert.ToInt32(r["Id"]),
                        PacienteID = Convert.ToInt32(r["PacienteID"]),
                        PaqueteID = Convert.ToInt32(r["PaqueteID"]),
                        PaqueteNombre = r["PaqueteNombre"] as string
                    }),
                    CommandType.StoredProcedure,
                    new[] { new SqlParameter("@PacienteId", pacienteId) });

                mr.IsSuccess = true;
                mr.Response = paquetes.ToList();
                mr.Message = "Paquetes obtenidos correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener los paquetes del paciente {Id}", pacienteId);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener los paquetes del paciente.";
            }
            return mr;
        }

        /// <summary>Lista las citas-tratamientos de un paquete del paciente. SP nuevo: sp_nh_GetCitasPorPacientePaquete.</summary>
        public ModelResponse<List<CitaPaqueteLecturaDTO>> ObtenerCitasPorPacientePaquete(int pacientePaqueteId, string usuario)
        {
            var mr = new ModelResponse<List<CitaPaqueteLecturaDTO>>();
            try
            {
                var citas = GetObjects("sp_nh_GetCitasPorPacientePaquete",
                    new Func<IDataReader, CitaPaqueteLecturaDTO>(r => new CitaPaqueteLecturaDTO
                    {
                        Id = Convert.ToInt32(r["Id"]),
                        FechaCita = Convert.IsDBNull(r["FechaCita"]) ? (DateTime?)null : Convert.ToDateTime(r["FechaCita"]),
                        TratamientoNombre = r["TratamientoNombre"] as string,
                        Comentarios = r["Comentarios"] as string,
                        CitaPaqueteID = Convert.ToInt32(r["CitaPaqueteID"]),
                        PacientePaqueteID = Convert.ToInt32(r["PacientePaqueteID"])
                    }),
                    CommandType.StoredProcedure,
                    new[] { new SqlParameter("@PacientePaqueteId", pacientePaqueteId) });

                mr.IsSuccess = true;
                mr.Response = citas.ToList();
                mr.Message = "Citas obtenidas correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener las citas del paquete {Id}", pacientePaqueteId);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener las citas del paquete.";
            }
            return mr;
        }
    }
}
