using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UptimeMailSender
{
    class Program
    {
        public static TimeSpan UpTime
        {
            get
            {
                using (var uptime = new PerformanceCounter("System", "System Up Time"))
                {
                    uptime.NextValue();
                    return TimeSpan.FromSeconds(uptime.NextValue());
                }
            }
        }

        private static DateTime lastMessageSent;

        static async Task Main(string[] args)
        {
            var configSource = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>(optional: true)
                .Build();

            var section = configSource.GetSection(nameof(Configuration));
            var config= section.Get<Configuration>();
            
            while (true)
            {
                await Task.Delay(5000);
                var now = DateTime.UtcNow;
                if (now > lastMessageSent.AddDays(1))
                {
                    SentMail(UpTime, config);
                    lastMessageSent = now;
                }
            }
        }

        private static void SentMail(TimeSpan uptime, Configuration config)
        {
            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress("UptimeCheck", config.FromMail));
            mailMessage.To.Add(new MailboxAddress("Owner", config.ToMail));
            mailMessage.Subject = $"{System.Net.Dns.GetHostName()} - Uptime: {uptime}";
            mailMessage.Body = new TextPart("plain")
            {
                Text = $"Uptime: {uptime}"
            };

            using var smtpClient = new SmtpClient();
            smtpClient.Connect(config.SmtpServer, 465, true);
            smtpClient.Authenticate(config.SmtpUser, config.SmtpPassword);
            smtpClient.Send(mailMessage);
            smtpClient.Disconnect(true);
        }
    }
}
