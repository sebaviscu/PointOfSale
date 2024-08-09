using Microsoft.AspNetCore.Http;
using PointOfSale.Business.Contracts;

namespace PintOfSale.FileStorageService.Servicios
{
    public class FileStorageService : IFileStorageService
    {
        private readonly ITiendaService _tiendaService;
        public FileStorageService(ITiendaService tiendaService)
        {
            _tiendaService = tiendaService;
        }

        public async Task SaveFileAsync(IFormFile file, string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = Path.Combine(directoryPath, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }

        public async Task DeleteFileIfExistsAsync(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        public async Task ReplaceFileAsync(IFormFile file, string directoryPath)
        {
            var filePath = Path.Combine(directoryPath, file.FileName);

            await DeleteFileIfExistsAsync(filePath);

            await SaveFileAsync(file, directoryPath);
        }

        public async Task<string> ObtenerRutaCertificadoAsync(int idTienda)
        {
            var tienda = await _tiendaService.Get(idTienda);

            if (tienda == null)
            {
                throw new Exception($"No se encontró la tienda con ID: {idTienda}");
            }

            if (string.IsNullOrEmpty(tienda.CertificadoNombre))
            {
                throw new Exception($"La tienda con ID: {idTienda} no tiene un certificado guardado.");
            }

            // Construye la ruta completa del certificado
            var nuevoProyectoPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "PintOfSale.FileStorageService");
            var certificadoPath = Path.Combine(nuevoProyectoPath, "Certificados", idTienda.ToString() + "_Tienda", tienda.CertificadoNombre);

            return certificadoPath;
        }
    }
}
