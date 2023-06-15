using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Tickets.Models;

namespace Tickets.Services
{
    public class TicketService : ITicketService
    {
        private readonly IConfigurationRoot _config;
        private readonly ILogService _logService;

        public TicketService(ILogService logService)
        {
            _config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            _logService = logService;
        }

        public void InsertTickets()
        {
            var pendientesPath = _config.GetSection("FolderPaths")["Pendientes"];
            var procesadosPath = _config.GetSection("FolderPaths")["Procesados"];
            foreach (var file in GetFiles(pendientesPath))
            {
                TextReader reader = new StreamReader(file);
                var ticketText = reader.ReadToEnd();
                try
                {
                    var ticket = TicketFormatted(ticketText);
                    InsertTicketDatabase(ticket);
                    File.Copy(file, $"{procesadosPath}\\{Path.GetFileName(file)}", true);
                    _logService.Add($"{file} {ticketText}");
                }
                catch (Exception ex)
                {
                    File.Copy(file, $"{procesadosPath}\\{Path.GetFileName(file)}_error", true);
                    _logService.Add($"{file} {ticketText} {ex.Message}");
                }
                reader.Close();
            }
        }

        private void InsertTicketDatabase(Ticket ticket)
        {
            using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("ConnectionString")))
            {
                using (SqlCommand cmd = new SqlCommand("InsertTicket", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdTienda", ticket.IdTienda);
                    cmd.Parameters.AddWithValue("@IdRegistradora", ticket.IdRegistradora);
                    cmd.Parameters.AddWithValue("@FechaHora", ticket.FechaHora);
                    cmd.Parameters.AddWithValue("@Ticket", ticket.NumeroTicket);
                    cmd.Parameters.AddWithValue("@Impuesto", ticket.Impuesto);
                    cmd.Parameters.AddWithValue("@Total", ticket.Total);
                    cmd.Parameters.AddWithValue("@FechaHoraCreacion", ticket.FechaHoraCreacion);
                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private Ticket TicketFormatted(string ticket)
        {
            var ticketFormatted = ticket.Trim().Substring(3).Split("|");
            return new Ticket
            {
                IdTienda = ticketFormatted.ElementAt(0),
                IdRegistradora = ticketFormatted.ElementAt(1),
                FechaHora = DateTimeFormatted(ticketFormatted.ElementAt(2), ticketFormatted.ElementAt(3)),
                NumeroTicket = int.Parse(ticketFormatted.ElementAt(4)),
                Impuesto = decimal.Parse(ticketFormatted.ElementAt(5)),
                Total = decimal.Parse(ticketFormatted.ElementAt(6)),
                FechaHoraCreacion = DateTime.Now
            };
        }

        private DateTime DateTimeFormatted(string date, string time)
        {
            var year = int.Parse(date.Substring(0, 4));
            var month = int.Parse(date.Substring(4, 2));
            var day = int.Parse(date.Substring(6, 2));
            var hour = int.Parse(time.Substring(0, 2));
            var minute = int.Parse(time.Substring(2, 2));
            var second = int.Parse(time.Substring(4, 2));
            return new DateTime(year, month, day, hour, minute, second);
        }

        private string[] GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }
    }
}
