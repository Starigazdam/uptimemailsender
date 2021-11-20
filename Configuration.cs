using System;
using System.Threading;
using MailKit.Security;

namespace UptimeMailSender
{
    public class Configuration
    {
        public string FromMail { get; set; }
        public string ToMail { get; set; }
        public string SmtpServer { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPassword { get; set; }
    }
}