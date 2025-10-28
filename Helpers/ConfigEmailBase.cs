using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Helpers
{
    public class ConfigEmailBase
    {
        public string SmtpServer { get; set; }

        public string SmtpUser { get; set; }

        public string SmtpPassword { get; set; }

        public string FromEmail { get; set; }

        public string FromText { get; set; }

        public string SmtpPortNumber { get; set; }

        public string TextFormat { get; set; }

        public string UrlRecuperarSenha { get; set; }

        public string ContactFormToEmail { get; set; }

        public string LocalImageContactForm { get; set; }

        public string FromEmailLanguiru { get; set; }

        public string FromTextLanguiru { get; set; }

        public string UrlRecuperarSenhaLanguiru { get; set; }

        public string ContactFormToEmailLanguiru { get; set; }
    }
}
