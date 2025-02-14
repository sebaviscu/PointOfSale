using Microsoft.AspNetCore.Mvc;

namespace PointOfSale.Web.Controllers
{
    public class LogsController : ControllerBase
    {
        private readonly string logDirectory;

        public LogsController(IWebHostEnvironment env)
        {
            logDirectory = Path.Combine(env.ContentRootPath, "logs");
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs()
        {
            // Obtener todos los archivos que comienzan con "logfile" y terminan con ".txt"
            var logFiles = Directory.GetFiles(logDirectory, "logfile*.txt");

            if (logFiles.Length == 0)
            {
                return NotFound("No hay logs disponibles.");
            }

            // Obtener el archivo más reciente
            var latestLogFile = logFiles.OrderByDescending(f => new FileInfo(f).CreationTime).First();

            try
            {
                // Abrir el archivo para leerlo sin bloquearlo
                using (var fs = new FileStream(latestLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fs))
                {
                    var logContent = await reader.ReadToEndAsync();
                    return Content(logContent, "text/plain");
                }
            }
            catch (IOException ex)
            {
                return StatusCode(500, $"Error al leer los logs: {ex.Message}");
            }
        }
    }
}
