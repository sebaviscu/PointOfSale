using AFIP.Facturacion.Model;
using Microsoft.AspNetCore.Http;
using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Contracts
{
    public interface IAfipService
    {
        Task<List<FacturaEmitida>> GetAll(int idTienda);

        Task<FacturaEmitida> GetById(int idFacturaEmitida);

        Task<string> GenerateLinkAfipFactura(FacturaEmitida factura);

        Task<string> ReplaceCertificateAsync(IFormFile file, int idTienda, string? oldCertificate);

        VMX509Certificate2 GetCertificateAfipInformation(string certificatePath, string certificatePassword);

        string ValidateCertificate(AjustesFacturacion? ajustes);

        Task CheckVencimientoCertificado(int idTienda);

        Task<FacturaEmitida> GetBySaleId(int idSale);

        Task<List<FacturaEmitida>> GetListBySaleId(int idSale);

        Task<FacturaEmitida?> FacturarVenta(Sale sale, Ajustes ajustes, string cuil, int? idCliente);

        Task<FacturaEmitida?> NotaCredito(int idFacturaemitida, string registrationUser);
        Task<FacturaEmitida?> Refacturar(int idFacturaemitida, string cuil, string registrationUser);

        Task<List<FacturaAFIP>> GetFacturaByVentas(List<Sale> sales, Ajustes ajustes, string cuil, int? idCliente);

        Task<FacturaEmitida> SaveFacturaEmitida(FacturacionResponse facturacion, int idSale);
    }
}
