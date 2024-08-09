using Microsoft.AspNetCore.Http;
using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AFIP.Facturacion.Services
{
    public interface IFileStorageService
    {
        Task<string> ReplaceCertificateAsync(IFormFile file, int idTienda);
        Task<string> ObtenerCertificadoAsync(AjustesFacturacion ajustesFacturacion);

        VMX509Certificate2 GetCertificateAfipInformation(string certificatePath, string certificatePassword);
    }
}
