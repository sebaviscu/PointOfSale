using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace PintOfSale.FileStorageService.Servicios
{
    public interface IFileStorageService
    {
        Task SaveFileAsync(IFormFile file, string filePath);
        Task DeleteFileIfExistsAsync(string filePath);
        Task ReplaceFileAsync(IFormFile file, string directoryPath);
        Task<string> ObtenerRutaCertificadoAsync(int idTienda);
    }
}
