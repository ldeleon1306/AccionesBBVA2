using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AccionesBBVA
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        public readonly IConfiguration _configuration;
        public string _connectionString { get; set; }
        private string _smtpServer { get; set; }
        private string _mailTo { get; set; }
        private string _mailFrom { get; set; }

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = _configuration["BBVA:ConnectionString"];
            _smtpServer = _configuration["MailSmtp:smtp"];
            _mailTo = _configuration["MailSmtp:mailTo"];
            _mailFrom = _configuration["MailSmtp:mailFrom"];
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var DaysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                var lastDay = new DateTime(now.Year, now.Month, DaysInMonth);
                //using (SqlConnection connection = new SqlConnection(@"Data Source=localhost,1401;Initial catalog=SERVERPROD;User ID=sa;Password=test@123;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
                //using (SqlConnection connection = new SqlConnection(@"Data Source=localhost,1401;Initial catalog=SERVERPROD;User ID=leonidas;Password=leonidas12345678910-;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
                //using (SqlConnection connection = new SqlConnection(@"Data Source=ITGDESAOCSRV.andreani.com.ar;Initial catalog=AccionesBBVA;Integrated Security=true"))
                //using (SqlConnection connection = new SqlConnection(@"Data Source=DBSCEFARMATEST;Initial catalog=LPNFD;Integrated Security=true"))
                {
                    //connection.Open();
                    //SqlDataReader sqlDr = null;
                    string queryString = "SELECT SUBSTRING(er.Registro,3,21) + SUBSTRING(er.Registro,85,3) + SUBSTRING(er.Registro,94,180)" +
                        "FROM AccionesBBVA..EntradaRegistros (nolock) ER " +
                        "where ER.codigoAccion IN (" +
                        "'003'," +
                        "'006'," +
                        "'016'" +
                        ")" +
                        "and er.fechaCreacion Between '2020-01-08 00:00:00' and '2022-12-09 00:00:00' " +
                        "and ER.Respuesta IN (" +
                        "4" +
                        ")" +
                        "and" +
                        "(" +
                        "er.observaciones like 'El estado del envio no permite realizar la operación. Envío en estado final. Estado: 6' " +
                        "OR  er.observaciones like 'El estado del envio no permite realizar la operación. Envío en estado final. Estado: 7' " +
                        "OR  er.observaciones like 'El estado del envio no permite realizar la operación. Envío en estado final. Estado: 8'" +
                        "OR  er.observaciones like 'No se pudo hallar el objeto: EntityNumber con identificador: G00000576967660'" +
                        ") " +
                        "AND not exists (" +
                        "select 1 from AccionesBBVA..EntradaRegistros (nolock) ER2 " +
                        "where " +
                        "er.NumeroInterno = er2.NumeroInterno " +
                        "and er2.respuesta <> 4 " +
                        "and er.codigoAccion = er2.codigoAccion " +
                        "and er.fechaCreacion < er2.fechaCreacion " +
                        "and er.id <> er2.id" +
                        ")";

                    DataTable dt = new DataTable();
                    int rows_returned;
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(_connectionString))
                        {
                            SqlCommand command = new SqlCommand(queryString, connection);
                            //command.Parameters.AddWithValue("@tPatSName", "Your-Parm-Value");
                            using (SqlDataAdapter sda = new SqlDataAdapter(command))
                            {
                                command.CommandText = queryString;
                                command.CommandType = CommandType.Text;
                                connection.Open();
                                rows_returned = sda.Fill(dt);
                                connection.Close();
                            }
                            if (dt.Rows.Count > 0)
                            {
                                foreach (var item in dt.AsEnumerable())
                                {
                                    Console.WriteLine(item.ItemArray[0]);
                                }
                            }
                            else
                            {


                            }
                        }

                        //using (SqlCommand cmd = new SqlCommand("sp_AccionesBBVA", connection))
                        //{
                        //    cmd.CommandType = CommandType.StoredProcedure;
                        //    cmd.CommandTimeout = 0;

                        //    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        //    {
                        //        DataTable dt = new DataTable();

                        //        da.Fill(dt);
                        //        foreach (var item in dt.AsEnumerable())
                        //        {
                        //            Console.WriteLine(item.ItemArray[0]);
                        //        }

                        //    }

                        //}

                        mail();
                        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                        await Task.Delay(600000, stoppingToken);
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }

                }

            }

            void mail()
            {
                MailMessage mail = new MailMessage();
                //Console.WriteLine(_mailFrom.ToString());
                //Console.WriteLine(_mailTo);
                mail.From = new MailAddress(_mailFrom);
                mail.To.Add(_mailTo);
                //var multiple = _mailTo.Split(';');
                //foreach (var to in multiple)
                //{
                //    if (to != string.Empty)
                //        mail.To.Add(to);
                //}              
                //System.Net.Mail.Attachment attachment;
                //attachment = new System.Net.Mail.Attachment("output.txt");
                //mail.Attachments.Add(attachment);
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
}
