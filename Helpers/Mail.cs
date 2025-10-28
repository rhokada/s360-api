using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace WebApi.Helpers
{
    public class Mail
    {
        public static bool Send(String toAddress, String toName, String ccAddress, String ccName, String subject, String body, ConfigEmailBase infos, bool ehLanguiru = false)
        {
            try
            {
                if (toAddress.Trim() != "")
                {
                    //Dados do E-mail
                    string FromAddress = ehLanguiru ? infos.FromEmailLanguiru : infos.FromEmail;
                    string FromAdressTitle = ehLanguiru ? infos.FromTextLanguiru : infos.FromText;
                    string ToAddress = toAddress.Trim(); // Adicione .Trim() aqui;
                    string ToAdressTitle = toName;
                    string CcAddress = ccAddress.Trim(); // Adicione .Trim() aqui;
                    string CcAddressTitle = ccName;
                    string Subject = subject;
                    string BodyContent = body;

                    //Smtp Server 
                    string SmtpServer = infos.SmtpServer;
                    int SmtpPortNumber = Convert.ToInt32(infos.SmtpPortNumber);


                    var mimeMessage = new MimeMessage();
                    mimeMessage.From.Add(new MailboxAddress(FromAdressTitle, FromAddress));
                    mimeMessage.To.Add(new MailboxAddress(ToAdressTitle, ToAddress));
                    mimeMessage.Subject = Subject;

//                    mimeMessage.Cc.Add(new MailboxAddress(CcAddressTitle, CcAddress)); //da problema quando cc = ""
                    if (!string.IsNullOrWhiteSpace(CcAddress))
                    {
                        mimeMessage.Cc.Add(new MailboxAddress(CcAddressTitle, CcAddress));
                    }

                    mimeMessage.Body = new TextPart("html")
                    {
                        Text = BodyContent

                    };

                    using (var client = new SmtpClient())
                    {

                        client.Connect(SmtpServer, SmtpPortNumber, false);

                        //Usuário e senha do smtp
                        client.Authenticate(infos.SmtpUser, infos.SmtpPassword);
                        client.Send(mimeMessage);
                        client.Disconnect(true);

                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return false;
        }
    }
}
