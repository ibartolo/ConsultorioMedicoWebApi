using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;
using Serilog;

namespace ConsultorioMedicoWebApi.DAL
{
    public partial class DbWrapper
    {
        /// <summary>Lista tipos de consulta. Reutiliza el SP existente GetAllTipoConsulta.</summary>
        public ModelResponse<List<TipoConsulta>> ObtenerTiposConsulta(string usuario)
        {
            var mr = new ModelResponse<List<TipoConsulta>>();
            try
            {
                var tipos = GetObjects("GetAllTipoConsulta",
                    new Func<IDataReader, TipoConsulta>(r => new TipoConsulta
                    {
                        Id = Convert.ToInt32(r["Id"]),
                        Nombre = r["Nombre"] as string,
                        Duracion = Convert.IsDBNull(r["Duracion"]) ? (int?)null : Convert.ToInt32(r["Duracion"]),
                        Descripcion = r["Descripcion"] as string,
                        Estatus = Convert.IsDBNull(r["Estatus"]) ? false : Convert.ToBoolean(r["Estatus"])
                    }),
                    CommandType.StoredProcedure);

                mr.IsSuccess = true;
                mr.Response = tipos.ToList();
                mr.Message = "Tipos de consulta obtenidos correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener tipos de consulta para {Usuario}", usuario);
                mr.IsSuccess = false;
                mr.Message = "Ocurrió un error al obtener los tipos de consulta.";
            }
            return mr;
        }
    }
}
