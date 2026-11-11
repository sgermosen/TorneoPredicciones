namespace CompeTournament.Backend.Helpers
{
    using MailKit.Net.Smtp;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using MimeKit;
    using System;

    public class MailHelper : IMailHelper
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MailHelper> _logger;

        public MailHelper(IConfiguration configuration, ILogger<MailHelper> logger)
        {
            this._configuration = configuration;
            this._logger = logger;
        }

        public bool SendMail(string to, string subject, string body)
        {
            var from = this._configuration["Mail:From"];
            var smtp = this._configuration["Mail:Smtp"];
            var port = this._configuration["Mail:Port"];
            var password = this._configuration["Mail:Password"];
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(smtp))
            {
                this._logger.LogWarning("El correo no esta configurado (Mail:From y Mail:Smtp); no se envio el mensaje.");
                return false;
            }

            try
            {
                var message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(from));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = body;
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.Connect(smtp, int.Parse(port), false);
                    client.Authenticate(from, password);
                    client.Send(message);
                    client.Disconnect(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "No se pudo enviar el correo.");
                return false;
            }
        }
    }
}
