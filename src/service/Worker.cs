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
        private string _tabla { get; set; }
        Mail m = new Mail();
        Querys q = new Querys();

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = _configuration["BBVA:ConnectionString"];
            _smtpServer = _configuration["MailSmtp:smtp"];
            _mailTo = _configuration["MailSmtp:mailTo"];
            _mailFrom = _configuration["MailSmtp:mailFrom"];
            _tabla = _configuration["BBVA:tabla"];
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //////////////////////Intervalo de fechas//////////////////////////////////////////////
                //calculate time to run the first time & delay to set the timer
                //DateTime.Today gives time of midnight 00.00
                var nextRunTime = DateTime.Today.AddDays(1).AddHours(9);
                var curTime = DateTime.Now;
                var firstInterval = nextRunTime.Subtract(curTime);
                //////////////////////Filtro de fechas//////////////////////////////////////////////
                var now = DateTime.Now;
                DateTime FechaRestada = now.AddDays(-7);
                DateTime FechaDesde = FechaRestada.Date.Add(new TimeSpan(0, 0, 0));
                DateTime FechaHasta = DateTime.Now.Date.Add(new TimeSpan(0, 0, 0));
                string dia = DateTime.Now.ToString("dddd");
               
                Console.WriteLine("nextRunTime: " + nextRunTime);
                Console.WriteLine("firstInterval: " + firstInterval);
                Console.WriteLine(dia);
                if (dia == "Monday")
                {
                    string queryString = q.query(FechaDesde.ToString("yyyy-MM-dd HH:mm:ss"), FechaHasta.ToString("yyyy-MM-dd HH:mm:ss"),_tabla);
                    Console.WriteLine(queryString);
                    {
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
                                    command.CommandTimeout = 0;
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
                            }
                            m.mail(_mailFrom, _mailTo, _smtpServer, dt.AsEnumerable());
                        
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            _logger.LogInformation(ex.ToString());                            
                        }

                    }
                }              
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(firstInterval, stoppingToken);

            }
        }
    }
}
