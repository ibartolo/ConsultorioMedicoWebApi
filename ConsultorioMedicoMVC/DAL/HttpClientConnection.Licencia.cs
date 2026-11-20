using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;

namespace ConsultorioMedicoMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<LicenciaDTO>> ObtenerLicencia()
            => await RequestAsync<LicenciaDTO>("api/Licencia", HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<List<ConsultorioPago>>> ObtenerPagos()
            => await RequestAsync<List<ConsultorioPago>>("api/Licencia/Pagos", HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<ConsultorioPago>> SubirComprobante(MultipartFormDataContent form)
            => await SendMultipartAsync<ConsultorioPago>("api/Licencia/SubirComprobante", form, token.Token.access_token);

        public async Task<byte[]> DescargarComprobante(int pagoId)
            => await DescargarBytesAsync("api/Licencia/Descargar/" + pagoId, token.Token.access_token);
    }
}
