using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ConsultorioMedicoEntities.Seguridad;
using ConsultorioMedicoMVC.Services;
using Newtonsoft.Json;

namespace ConsultorioMedicoMVC.Controllers
{
    /// <summary>
    /// Recuperación de contraseña (público). Sin [Authorize]; el filtro lo permite vía PublicActions.
    /// </summary>
    public class RecuperacionPasswordController : BaseController
    {
        private readonly RecuperacionPasswordService _service;

        public RecuperacionPasswordController() : base()
        {
            _service = new RecuperacionPasswordService(httpClientConnection);
        }

        // POST: RecuperacionPassword/Solicitar (público) → devuelve JSON para el modal del login.
        [HttpPost]
        public async Task<string> Solicitar(string email)
        {
            var response = await _service.Solicitar(email);
            return JsonConvert.SerializeObject(response);
        }

        // GET: RecuperacionPassword/Restablecer?token=... (público)
        public async Task<ActionResult> Restablecer(string token)
        {
            var response = await _service.Validar(token);
            if (!response.IsSuccess)
            {
                ViewBag.Message = response.Message ?? "El enlace es inválido o ha expirado.";
                return View("Invalido");
            }

            ViewBag.Token = token;
            return View();
        }

        // POST: RecuperacionPassword/Guardar (público) → devuelve JSON para la vista.
        [HttpPost]
        public async Task<string> Guardar(string token, string password, string confirmacion)
        {
            // Validación en servidor (además de la del cliente).
            var passwordError = PasswordPolicy.Validar(password);
            if (passwordError != null)
                return JsonConvert.SerializeObject(new ModelResponse
                {
                    IsSuccess = false,
                    Message = passwordError
                });

            if (!string.Equals(password, confirmacion, StringComparison.Ordinal))
                return JsonConvert.SerializeObject(new ModelResponse
                {
                    IsSuccess = false,
                    Message = "La confirmación de la contraseña no coincide."
                });

            var response = await _service.Restablecer(token, password, confirmacion);
            return JsonConvert.SerializeObject(response);
        }
    }
}
