using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace School_CRM.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpEmailAsync(string toEmail, string userName, string otpCode, int expiryMinutes = 3)
        {
            var host = _config["SmtpSettings:Host"] ?? "smtp.gmail.com";
            var portStr = _config["SmtpSettings:Port"] ?? "587";
            var enableSslStr = _config["SmtpSettings:EnableSsl"] ?? "true";
            var senderName = _config["SmtpSettings:SenderName"] ?? "Krishak Inter College Security Centre";
            var senderEmail = _config["SmtpSettings:SenderEmail"];
            var password = _config["SmtpSettings:Password"];

            if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("SMTP credentials are not configured in appsettings.json.");
            }

            int port = int.TryParse(portStr, out var p) ? p : 587;
            bool enableSsl = !bool.TryParse(enableSslStr, out var ssl) || ssl;

            using (var mail = new MailMessage())
            {
                mail.From = new MailAddress(senderEmail, senderName);
                mail.To.Add(new MailAddress(toEmail));
                mail.Subject = "Krishak Inter College School CRM - Login OTP Verification Code";
                mail.IsBodyHtml = true;

                // Professional Premium HTML Body
                mail.Body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Login OTP Verification</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f4f6f9;
            color: #333;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 500px;
            margin: 40px auto;
            background: #ffffff;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 4px 15px rgba(0,0,0,0.05);
            border: 1px solid #eef2f5;
        }}
        .header {{
            background: linear-gradient(135deg, #25A194 0%, #1d8277 100%);
            padding: 30px;
            text-align: center;
            color: #ffffff;
        }}
        .header h2 {{
            margin: 0;
            font-size: 22px;
            font-weight: 600;
            letter-spacing: 0.5px;
        }}
        .content {{
            padding: 35px 30px;
            line-height: 1.6;
        }}
        .content p {{
            margin-top: 0;
            margin-bottom: 20px;
            color: #555555;
            font-size: 15px;
        }}
        .otp-box {{
            background-color: #f0f7f6;
            border: 1px dashed #25A194;
            border-radius: 8px;
            padding: 15px;
            text-align: center;
            margin: 25px 0;
        }}
        .otp-code {{
            font-size: 32px;
            font-weight: 700;
            letter-spacing: 6px;
            color: #25A194;
            margin: 0;
        }}
        .timer-info {{
            font-size: 13px;
            color: #e53e3e;
            font-weight: 600;
            text-align: center;
            margin-top: 5px;
        }}
        .footer {{
            background-color: #fafbfc;
            padding: 20px;
            text-align: center;
            font-size: 12px;
            color: #999;
            border-top: 1px solid #f0f2f5;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Two-Factor Authentication</h2>
        </div>
        <div class='content'>
            <p>Hello <strong>{userName}</strong>,</p>
            <p>We received a request to log in to your <strong>Krishak Inter College School CRM</strong> account. Please use the following One-Time Password (OTP) to complete your verification:</p>
            
            <div class='otp-box'>
                <div class='otp-code'>{otpCode}</div>
                <div class='timer-info'>⏱️ Valid for {expiryMinutes} minutes only</div>
            </div>

            <p style='margin-bottom: 0;'>If you did not request this, you can safely ignore this email. We recommend changing your password if you suspect unauthorized access.</p>
        </div>
        <div class='footer'>
            &copy; {DateTime.Now.Year} Krishak Inter College. All rights reserved.<br>
            This is an automated security notification. Please do not reply directly.
        </div>
    </div>
</body>
</html>";

                using (var smtp = new SmtpClient(host, port))
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(senderEmail, password);
                    smtp.EnableSsl = enableSsl;
                    await smtp.SendMailAsync(mail);
                }
            }
        }
    }
}
