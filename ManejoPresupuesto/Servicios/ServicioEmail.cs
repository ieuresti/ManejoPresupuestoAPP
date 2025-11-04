using System.Net;
using System.Net.Mail;

namespace ManejoPresupuesto.Servicios
{
    public interface IServicioEmail
    {
        Task EnviarEmailCambioPassword(string receptor, string enlace);
    }
    public class ServicioEmail: IServicioEmail
    {
        private readonly IConfiguration configuration;
        public ServicioEmail(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task EnviarEmailCambioPassword(string receptor, string enlace)
        {
            // Leer valores de configuración (recomendado guardarlos en User Secrets o variables de entorno)
            var email = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:EMAIL");
            var password = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:PASSWORD");
            var host = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:HOST");
            var puerto = configuration.GetValue<int>("CONFIGURACIONES_EMAIL:PUERTO");
            // Crear y configurar el cliente SMTP
            // Se establece SSL y las credenciales para autenticarse en el servidor SMTP
            var cliente = new SmtpClient(host, puerto);
            cliente.EnableSsl = true;
            cliente.UseDefaultCredentials = false;
            cliente.Credentials = new NetworkCredential(email, password);
            // Construir el contenido del mensaje
            var emisor = email;
            var subject = "¿Ha olvidado su contraseña?";
            var contenidoHtml = $@"Saludos,

            Este mensaje le llega porque usted ha solicitado un cambio de contraseña. Si esta solicitud no fue hecha por usted, puede ignorar este mensaje.

            Para cambiar su contraseña, haga click en el siguiente enlace:

            {enlace}

            Atentamente,
            Equipo Manejo Presupuesto";

            // Crear el MailMessage con emisor, receptor, asunto y cuerpo
            var mensaje = new MailMessage(emisor, receptor, subject, contenidoHtml)
            {
                // Asegurar codificación UTF-8 para caracteres especiales
                BodyEncoding = System.Text.Encoding.UTF8,
                SubjectEncoding = System.Text.Encoding.UTF8,
                IsBodyHtml = false // cambiar a true si el cuerpo contiene HTML
            };
            // Enviar el correo de forma asíncrona
            await cliente.SendMailAsync(mensaje);
        }
    }
}
