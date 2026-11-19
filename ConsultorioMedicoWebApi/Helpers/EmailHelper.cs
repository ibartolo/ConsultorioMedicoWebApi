using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace ConsultorioMedicoWebApi.Helpers
{
    /// <summary>
    /// Envío de correo saliente vía SMTP corporativo (SmarterASP).
    /// Portado de ServiceDeskDESIWebApi/Helpers/EmailHelper.cs (método renombrado a EnviarEmail).
    /// Configuración en Web.config: userEmail, passEmail, smtpClient, port.
    /// </summary>
    public class EmailHelper
    {
        public static void EnviarEmail(IEnumerable<string> para, string asunto, string mensaje, string attachment = "")
        {
            try
            {
                var de = ConfigurationManager.AppSettings["userEmail"];
                var pass = ConfigurationManager.AppSettings["passEmail"];
                var smtpURL = ConfigurationManager.AppSettings["smtpClient"];
                var puerto = Convert.ToInt32(ConfigurationManager.AppSettings["port"]);

                var mail = new MailMessage
                {
                    From = new MailAddress(de, "Consultorio Médico"),
                    IsBodyHtml = true,
                    Subject = asunto,
                    SubjectEncoding = Encoding.UTF8,
                    Body = mensaje,
                    BodyEncoding = Encoding.UTF8
                };

                foreach (var p in para)
                {
                    if (!string.IsNullOrWhiteSpace(p))
                        mail.To.Add(p);
                }

                // En caso de anexos.
                if (!string.IsNullOrEmpty(attachment))
                    mail.Attachments.Add(new Attachment(attachment));

                using (var smtp = new SmtpClient(smtpURL))
                {
                    smtp.EnableSsl = true; // SMTP corporativo requiere SSL.
                    smtp.Port = puerto;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false; // Se usan credenciales específicas.
                    smtp.Timeout = 15000; // No bloquear el alta más de 15 s si el SMTP falla.

                    smtp.Credentials = new NetworkCredential(de, pass);

                    if (mail.To.Count != 0)
                        smtp.Send(mail);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
