using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ConsultorioMedicoEntities.Seguridad;
using Newtonsoft.Json;

namespace ConsultorioMedicoMVC.DAL
{
    /// <summary>
    /// Motor HTTP genérico. Una instancia por request; nunca lanza excepciones al consumidor:
    /// ante error HTTP devuelve ModelResponse con IsSuccess=false.
    /// </summary>
    public class HttpClientBase
    {
        private readonly HttpClient httpClient;
        public string BaseUri { get; private set; }

        public HttpClientBase(string baseUrl)
        {
            BaseUri = string.IsNullOrEmpty(baseUrl)
                ? ConfigurationManager.AppSettings["BaseUriWebApi"]
                : baseUrl;

            httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUri),
                Timeout = TimeSpan.FromMinutes(5)
            };
        }

        private void SetParametersHttpCliente(string contentType, string token)
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            if (!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>Obtiene token OAuth (password grant, application/x-www-form-urlencoded).</summary>
        public async Task<T> TokenAsync<T>(string endpoint, IEnumerable<KeyValuePair<string, string>> content, string contentType = "application/x-www-form-urlencoded")
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using (var requestContent = new FormUrlEncodedContent(content))
            using (var response = await httpClient.PostAsync(endpoint, requestContent))
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        /// <summary>Request tipado estándar que devuelve ModelResponse&lt;TResponse&gt;.</summary>
        public async Task<ModelResponse<TResponse>> RequestAsync<TResponse>(string endpoint, HttpMethod method, object content, string token, string contentType = "application/json")
        {
            try
            {
                SetParametersHttpCliente(contentType, token);
                using (var request = new HttpRequestMessage(method, endpoint))
                {
                    if (content != null && (method == HttpMethod.Post || method == HttpMethod.Put))
                        request.Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, contentType);

                    using (var response = await httpClient.SendAsync(request))
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        if (response.IsSuccessStatusCode)
                            return JsonConvert.DeserializeObject<ModelResponse<TResponse>>(json);

                        return new ModelResponse<TResponse>
                        {
                            IsSuccess = false,
                            Message = string.Format("Error {0} ({1}) al consumir {2}.", (int)response.StatusCode, response.ReasonPhrase, endpoint)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new ModelResponse<TResponse> { IsSuccess = false, Message = "Error de conexión: " + ex.Message };
            }
        }

        /// <summary>Envía contenido multipart/form-data (subida de archivos) y devuelve ModelResponse&lt;T&gt;.</summary>
        public async Task<ModelResponse<T>> SendMultipartAsync<T>(string endpoint, MultipartFormDataContent content, string token = "")
        {
            SetParametersHttpCliente("application/json", token);
            try
            {
                using (var response = await httpClient.PostAsync(endpoint, content))
                {
                    var json = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                        return JsonConvert.DeserializeObject<ModelResponse<T>>(json);

                    return new ModelResponse<T>
                    {
                        IsSuccess = false,
                        Message = string.Format("Error {0} ({1}) al consumir {2}.", (int)response.StatusCode, response.ReasonPhrase, endpoint)
                    };
                }
            }
            catch (Exception ex)
            {
                return new ModelResponse<T> { IsSuccess = false, Message = "Error de conexión: " + ex.Message };
            }
        }

        /// <summary>Descarga un archivo (bytes) desde la WebApi.</summary>
        public async Task<byte[]> DescargarBytesAsync(string endpoint, string token = "")
        {
            SetParametersHttpCliente("application/json", token);
            try
            {
                using (var response = await httpClient.GetAsync(endpoint))
                {
                    if (response.IsSuccessStatusCode)
                        return await response.Content.ReadAsByteArrayAsync();
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
