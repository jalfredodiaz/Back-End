using Back_End.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Back_End.Servicios
{
    public class EmailSender : IEmailSender
    {
        private SmtpClient Cliente { get; }
        private EmailSenderOptions Options { get; set; }

        public EmailSender()
        {
            Options = new EmailSenderOptions(); //Valores del correo que envia el mensaje de restablecer usuario/contraseña
            Cliente = new SmtpClient()
            {
                Host = Options.Host,
                Port = Options.Port,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(Options.Email, Options.Password),
                EnableSsl = Options.EnableSsl,
            };
        }

        public Task SendEmailAsync(string email, string tituloMensaje, string cuerpoMensaje, string nombreQuienEnvia = "")
        {
            var correo = new MailMessage();// (from: Options.Email, to: email, subject: tituloMensaje, body: cuerpoMensaje);

            if (nombreQuienEnvia != null && nombreQuienEnvia.Trim().Length > 0)
            {
                correo.From = new MailAddress(Options.Email, nombreQuienEnvia);
            } else
            {
                correo.From = new MailAddress(Options.Email);
            }

            var correos = email.Split(';');

            foreach (string sCorreo in correos)
            {
                correo.To.Add(new MailAddress(sCorreo));
            }

            correo.Subject = tituloMensaje;
            correo.Body = cuerpoMensaje;
            correo.IsBodyHtml = true;

            return Cliente.SendMailAsync(correo);
        }
    }
}
