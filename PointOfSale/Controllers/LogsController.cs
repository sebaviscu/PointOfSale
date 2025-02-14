using Microsoft.AspNetCore.Mvc;
using PointOfSale.Controllers;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Web.Controllers
{
    public class LogsController : BaseController
    {
        private readonly string logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");

        [HttpGet()]
        public async Task<IActionResult> GetLogs()
        {
            var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");

            if (!Directory.Exists(logsDirectory))
            {
                return NotFound("No hay logs disponibles.");
            }

            var logFiles = Directory.GetFiles(logsDirectory, "logfile*.txt");

            if (logFiles.Length == 0)
            {
                return NotFound("No hay logs disponibles.");
            }

            var latestLogFile = logFiles.OrderByDescending(f => f).FirstOrDefault();

            if (latestLogFile == null)
            {
                return NotFound("No se encontró el archivo de logs.");
            }

            try
            {
                using var stream = new FileStream(latestLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                var logs = await reader.ReadToEndAsync();
                return Content(logs, "text/plain");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al leer los logs: {ex.Message}");
            }
        }


        [HttpDelete()]
        public IActionResult DeleteOldLogs()
        {
            ValidarAutorizacion([Roles.Administrador]);
            try
            {

                string[] logFiles = Directory.GetFiles(logsDirectory, "logfile*.txt");
                string todayLog = Path.Combine(logsDirectory, $"logfile{DateTime.Now:yyyyMMdd}.txt");

                foreach (var logFile in logFiles)
                {
                    if (logFile != todayLog) System.IO.File.Delete(logFile);
                }

                return Ok("Logs viejos eliminados correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al eliminar los logs: {ex.Message}");
            }
        }
    }
}
