using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using ConsultorioMedicoEntities;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoMVC.Helpers;

namespace ConsultorioMedicoMVC.DAL
{
    public partial class HttpClientConnection : HttpClientBase
    {
        private readonly TokenCookie token;

        public HttpClientConnection(string baseUrl = "") : base(baseUrl)
        {
            token = SessionHelper.GetSessionUser();   // NULL antes del login
        }

        /// <summary>Auditoría de cliente: sella CreadoPor/FechaCreacion o ModificadoPor/FechaModificacion según Id.</summary>
        public BaseObject MappingColumSecurity(BaseObject o)
        {
            if (o.Id == 0 || o.Id == -1)
            {
                o.CreatedBY = SessionHelper.GetSessionUser()?.UserName;
                o.CreatedDT = SessionHelper.GetDateCenterMexico();
            }
            else
            {
                o.UpdatedBY = SessionHelper.GetSessionUser()?.UserName;
                o.UpdatedDT = SessionHelper.GetDateCenterMexico();
            }
            return o;
        }

        public async Task<Token> GetToken(string user, string pass)
            => await TokenAsync<Token>("token", new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("UserName", user),
                new KeyValuePair<string, string>("Password", pass),
                new KeyValuePair<string, string>("client_id", ConfigurationManager.AppSettings["client_id"]),
                new KeyValuePair<string, string>("client_secret", ConfigurationManager.AppSettings["client_secret"])
            }, "application/x-www-form-urlencoded");
    }
}
