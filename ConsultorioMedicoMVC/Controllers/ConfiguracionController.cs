using System.Web.Mvc;

namespace ConsultorioMedicoMVC.Controllers
{
    public class ConfiguracionController : BaseController
    {
        public ActionResult Index()
        {
            var tema = Request.Cookies["tema"] != null ? Request.Cookies["tema"].Value : "claro";
            if (tema != "claro" && tema != "oscuro") tema = "claro";
            ViewBag.Tema = tema;
            return View();
        }
    }
}
