using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ConsultorioMedicoEntities.Autenticacion;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using Serilog;

namespace ConsultorioMedicoWebApi.DAL
{
    /// <summary>
    /// Acceso a datos de licencia y pagos del consultorio (SaaS).
    /// El tenant (empresaId) proviene siempre del claim OAuth, nunca del cliente.
    /// sp_nh_ConfirmarPagoLicencia NO se expone (confirmación manual).
    /// </summary>
    public partial class DbWrapper
    {
        private LicenciaDTO LlenarLicencia(IDataReader r)
        {
            return new LicenciaDTO
            {
                ConsultorioID = Convert.ToInt32(r["ConsultorioID"]),
                Nombre = r["Nombre"] as string,
                Estatus = Convert.ToBoolean(r["Estatus"]),
                FechaVigenciaInicio = Convert.IsDBNull(r["FechaVigenciaInicio"]) ? (DateTime?)null : Convert.ToDateTime(r["FechaVigenciaInicio"]),
                FechaVigenciaFin = Convert.IsDBNull(r["FechaVigenciaFin"]) ? (DateTime?)null : Convert.ToDateTime(r["FechaVigenciaFin"]),
                EsPeriodoPrueba = Convert.ToBoolean(r["EsPeriodoPrueba"]),
                Plan = r["Plan"] as string
            };
        }

        private ConsultorioPago LlenarConsultorioPago(IDataReader r)
        {
            return new ConsultorioPago
            {
                Id = Convert.ToInt32(r["Id"]),
                ConsultorioID = Convert.ToInt32(r["ConsultorioID"]),
                NombreArchivo = r["NombreArchivo"] as string,
                ArchivoRuta = r["ArchivoRuta"] as string,
                Monto = Convert.ToDecimal(r["Monto"]),
                EstatusPagoId = Convert.ToInt32(r["EstatusPagoId"]),
                FechaCarga = Convert.ToDateTime(r["FechaCarga"]),
                SubidoPor = r["SubidoPor"] as string,
                FechaConfirmacion = Convert.IsDBNull(r["FechaConfirmacion"]) ? (DateTime?)null : Convert.ToDateTime(r["FechaConfirmacion"]),
                ConfirmadoPor = r["ConfirmadoPor"] as string,
                Notas = r["Notas"] as string,
                Estatus = Convert.ToBoolean(r["Estatus"])
            };
        }

        /// <summary>Obtiene la licencia del tenant. SP: sp_nh_GetLicencia.</summary>
        public ModelResponse<LicenciaDTO> ObtenerLicencia(int empresaId)
        {
            var mr = new ModelResponse<LicenciaDTO>();
            try
            {
                var licencia = GetObject("sp_nh_GetLicencia", LlenarLicencia, CommandType.StoredProcedure,
                    new[] { new SqlParameter("@ConsultorioID", empresaId) });

                if (licencia == null)
                {
                    mr.IsSuccess = false;
                    mr.Message = "No se encontró la licencia del consultorio.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = licencia;
                mr.Message = "Licencia obtenida correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener licencia del consultorio {Id}", empresaId);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener la licencia.";
            }
            return mr;
        }

        /// <summary>Lista el historial de pagos activos del tenant. SP: sp_nh_GetPagosLicencia.</summary>
        public ModelResponse<List<ConsultorioPago>> ObtenerPagosLicencia(int empresaId)
        {
            var mr = new ModelResponse<List<ConsultorioPago>>();
            try
            {
                var pagos = GetObjects("sp_nh_GetPagosLicencia", LlenarConsultorioPago, CommandType.StoredProcedure,
                    new[] { new SqlParameter("@ConsultorioID", empresaId) });

                mr.IsSuccess = true;
                mr.Response = pagos.ToList();
                mr.Message = "Pagos obtenidos correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener pagos del consultorio {Id}", empresaId);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener los pagos.";
            }
            return mr;
        }

        /// <summary>
        /// Inserta un pago Pendiente. SP: sp_nh_GuardarPagoLicencia. Devuelve el nuevo Id o 0.
        /// El SP también extiende FechaVigenciaFin al fin del día si la licencia venció.
        /// </summary>
        public int GuardarPagoLicencia(int empresaId, string nombreArchivo, string archivoRuta, decimal monto, string usuario)
        {
            try
            {
                var id = ExecuteScalar("sp_nh_GuardarPagoLicencia", CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@ConsultorioID", empresaId),
                        new SqlParameter("@NombreArchivo", (object)nombreArchivo ?? DBNull.Value),
                        new SqlParameter("@ArchivoRuta", (object)archivoRuta ?? DBNull.Value),
                        new SqlParameter("@Monto", monto),
                        new SqlParameter("@Usuario", (object)usuario ?? DBNull.Value)
                    });

                if (id == null || id == DBNull.Value) return 0;
                return Convert.ToInt32(id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar pago de licencia para {Usuario}", usuario);
                return 0;
            }
        }

        /// <summary>Obtiene un pago por Id validando tenant. SP: sp_nh_GetPagoLicenciaById.</summary>
        public ModelResponse<ConsultorioPago> ObtenerPagoLicenciaById(int pagoId, int empresaId)
        {
            var mr = new ModelResponse<ConsultorioPago>();
            try
            {
                var pago = GetObject("sp_nh_GetPagoLicenciaById", LlenarConsultorioPago, CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@PagoId", pagoId),
                        new SqlParameter("@ConsultorioID", empresaId)
                    });

                if (pago == null)
                {
                    mr.IsSuccess = false;
                    mr.Message = "Comprobante no encontrado.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = pago;
                mr.Message = "Comprobante obtenido correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener comprobante {Id}", pagoId);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener el comprobante.";
            }
            return mr;
        }

        /// <summary>Obtiene el primer usuario (menor Id, activo) del tenant. SP: sp_nh_GetPrimerUsuarioConsultorio.</summary>
        public ModelResponse<Usuario> ObtenerPrimerUsuarioConsultorio(int empresaId)
        {
            var mr = new ModelResponse<Usuario>();
            try
            {
                var usuario = GetObject("sp_nh_GetPrimerUsuarioConsultorio",
                    new Func<IDataReader, Usuario>(r => new Usuario
                    {
                        Id = Convert.ToInt32(r["Id"]),
                        UserName = r["UserName"] as string,
                        Email = r["Email"] as string
                    }),
                    CommandType.StoredProcedure,
                    new[] { new SqlParameter("@ConsultorioID", empresaId) });

                if (usuario == null)
                {
                    mr.IsSuccess = false;
                    mr.Message = "No se encontró el usuario del consultorio.";
                    return mr;
                }

                mr.IsSuccess = true;
                mr.Response = usuario;
                mr.Message = "Usuario obtenido correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener primer usuario del consultorio {Id}", empresaId);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener el usuario.";
            }
            return mr;
        }
    }
}
