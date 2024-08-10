using Microsoft.AspNetCore.Http;
using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface IAfipService
    {
        Task<FacturaEmitida> Facturar(Sale sale_created, int? nroDocumento, int? idCliente, string registrationUser);

        Task<List<FacturaEmitida>> GetAll(int idTienda);

        Task<FacturaEmitida> GetById(int idFacturaEmitida);

        Task<string> GenerateFacturaQR(FacturaEmitida factura);

        Task<string> ReplaceCertificateAsync(IFormFile file, int idTienda, string? oldCertificate);

        VMX509Certificate2 GetCertificateAfipInformation(string certificatePath, string certificatePassword);

        string ValidateCertificate(AjustesFacturacion? ajustes);

        Task CheckVencimientoCertificado(int idTienda);
    }
}
