using System;
using System.Globalization;
using System.Web.Mvc;

namespace ConsultorioMedicoMVC.App_Start
{
    /// <summary>
    /// Model binder para DateTime/DateTime? que acepta el formato "yyyy-MM-dd" del
    /// input type="date" de HTML5 (y otros formatos ISO), independiente de la cultura.
    /// </summary>
    public class DateTimeModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (value == null || string.IsNullOrWhiteSpace(value.AttemptedValue))
                return null;

            var attempted = value.AttemptedValue.Trim();

            DateTime parsed;
            if (DateTime.TryParseExact(attempted, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                return parsed;
            if (DateTime.TryParseExact(attempted, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                return parsed;
            if (DateTime.TryParseExact(attempted, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                return parsed;
            if (DateTime.TryParseExact(attempted, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                return parsed;
            if (DateTime.TryParse(attempted, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                return parsed;

            return null;
        }
    }
}
