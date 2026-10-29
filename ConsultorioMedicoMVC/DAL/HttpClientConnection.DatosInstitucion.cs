using System.Net.Http;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Catalogos;
using ConsultorioMedicoEntities.Seguridad;

namespace ConsultorioMedicoMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<DatosInstitucion>> ObtenerDatosInstitucion(int id)
            => await RequestAsync<DatosInstitucion>("api/DatosInstitucion/" + id, HttpMethod.Get, null, token.Token.access_token);

        public async Task<ModelResponse<DatosInstitucion>> GuardarDatosInstitucion(DatosInstitucion datos)
            => await RequestAsync<DatosInstitucion>("api/DatosInstitucion/Guardar", HttpMethod.Post, datos, token.Token.access_token);

        public async Task<ModelResponse<DatosInstitucion>> SubirLogo(MultipartFormDataContent form)
            => await SendMultipartAsync<DatosInstitucion>("api/DatosInstitucion/SubirLogo", form, token.Token.access_token);

        public async Task<byte[]> DescargarLogo(int id)
            => await DescargarBytesAsync("api/DatosInstitucion/Logo/" + id, token.Token.access_token);
    }
}
