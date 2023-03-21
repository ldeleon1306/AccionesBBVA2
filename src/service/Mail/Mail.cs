using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Mail;
using System.Text;

namespace AccionesBBVA
{
   public class Mail
    {
        public void mail(string _mailFrom, string _mailTo, string _smtpServer, EnumerableRowCollection<DataRow> dataRows)
        {
            string fecha = DateTime.Now.ToString("yyMMdd");
            string myfile = @"/tmp/MODIF-ACCIONES-F"+ fecha + ".txt";

            // Overwriting to the above existing file
            using (StreamWriter sw = File.CreateText(myfile))
            {
                foreach (var item in dataRows)
                {
                    sw.WriteLine(item.ItemArray[0]);
                }              
            }
            // Opening the file for reading
            using (StreamReader sr = File.OpenText(myfile))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    Console.WriteLine(s);
                }
            }

            MailMessage mail = new MailMessage();
            //Console.WriteLine(_mailFrom.ToString());
            //Console.WriteLine(_mailTo);
            mail.From = new MailAddress(_mailFrom);
            mail.To.Add(_mailTo);
            var multiple = _mailTo.Split(';');
            foreach (var to in multiple)
            {
                if (to != string.Empty)
                    mail.To.Add(to);
            }
            System.Net.Mail.Attachment attachment;
            attachment = new System.Net.Mail.Attachment(myfile);
            mail.Attachments.Add(attachment);
            //if (!String.IsNullOrEmpty(txterror))
            //{
            //  attachment = new System.Net.Mail.Attachment(txterror);
            //  mail.Attachments.Add(attachment);
            //}
            string cliente = "lein";
            string subject = string.Format($"Prueba {cliente} Mail");
            string bodyMsg = string.Format($"Se procesó un mongo {cliente}");
            mail.Subject = subject;
            mail.Body = bodyMsg;
            mail.IsBodyHtml = true;
            //Console.WriteLine(bodyMsg);
            SmtpClient smtp = new SmtpClient(_smtpServer);
            smtp.EnableSsl = false;
            smtp.Port = 25;
            smtp.UseDefaultCredentials = true;
            //string user = "leosendmailoe@gmail.com";
            //string pass = "ordenexterna";
            //NetworkCredential userCredential = new NetworkCredential(user, pass);

            //smtp.Credentials = userCredential;           
            smtp.Send(mail);
        }
     
    }
}
