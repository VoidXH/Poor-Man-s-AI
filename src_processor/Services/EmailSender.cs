using System.Net;
using System.Net.Mail;

using PoorMansAI.Configuration;

namespace PoorMansAI.Services;

/// <summary>
/// Sends emails via SMTP using configuration values from the active configuration file.
/// </summary>
public static class EmailSender {
    /// <summary>
    /// Sends an email to the specified recipient.
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body</param>
    /// <returns>A success or failure message.</returns>
    public static void Send(string to, string subject, string body) {
        using SmtpClient smtp = new() {
            Host = Config.agentEmailServer,
            Port = Config.agentSmtpPort,
            EnableSsl = Config.agentSmtpStartTLS,
            Credentials = new NetworkCredential(Config.agentEmailUser, Config.agentEmailPassword)
        };

        using MailMessage mail = new() {
            From = new MailAddress(Config.agentEmailUser),
            Subject = subject ?? string.Empty,
            Body = body ?? string.Empty,
            IsBodyHtml = false
        };
        mail.To.Add(to);
        smtp.Send(mail);
    }
}
