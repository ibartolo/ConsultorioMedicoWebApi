using System.Net.Http;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;

namespace ConsultorioMedicoMVC.DAL
{
    public partial class HttpClientConnection
    {
        // La WebApi resuelve el tenant desde el claim; no se envía id desde el cliente.
        public async Task<ModelResponse<DatosInstitucion>> ObtenerDatosInstitucion()
            => await RequestAsync<DatosInstitucion>("api/DatosInstitucion", HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<DatosInstitucion>> GuardarDatosInstitucion(DatosInstitucion datos)
            => await RequestAsync<DatosInstitucion>("api/DatosInstitucion/Guardar", HttpMethod.Post, datos, token.Token.access_token);

        public async Task<ModelResponse<DatosInstitucion>> SubirLogo(MultipartFormDataContent form)
            => await SendMultipartAsync<DatosInstitucion>("api/DatosInstitucion/SubirLogo", form, token.Token.access_token);

        public async Task<byte[]> DescargarLogo()
            => await DescargarBytesAsync("api/DatosInstitucion/Logo", token.Token.access_token);
    }
}
