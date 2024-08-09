using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFIP.Facturacion.Model;
using PointOfSale.Model.Afip.Factura;
using AFIP.Facturacion.Services;
using PointOfSale.Business.Contracts;
using AFIP.Facturacion.Extensions;
using PointOfSale.Business.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static PointOfSale.Model.Enum;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography.X509Certificates;

namespace PointOfSale.Business.Services
{
    public class AfipService : IAfipService
    {
        const int ptoVenta = 1;
        const int defaultDocumento = 0;
        const string URL_AFIP_QR = "https://www.afip.gob.ar/fe/qr/?p=";

        private readonly IGenericRepository<FacturaEmitida> _repository;
        private readonly IAFIPFacturacionService _afipFacturacionService;
        private readonly IAjusteService _ajusteService;
        private readonly IFileStorageService _fileStorageService;

        public AfipService(IGenericRepository<FacturaEmitida> repository, IAFIPFacturacionService afipFacturacionService, IAjusteService ajusteService, IFileStorageService fileStorageService)
        {
            _repository = repository;
            _afipFacturacionService = afipFacturacionService;
            _ajusteService = ajusteService;
            _fileStorageService = fileStorageService;
        }

        public async Task<FacturaEmitida> Facturar(Sale sale_created, int? nroDocumento, int? idCliente, string registrationUser)
        {
            if (sale_created.TypeDocumentSaleNavigation == null)
            {
                throw new Exception("El tipo de documento no existe");
            }

            var documentoAFacturar = (sale_created.TypeDocumentSaleNavigation.TipoFactura == TipoFactura.A && nroDocumento.HasValue)
                ? nroDocumento.Value
                : defaultDocumento;

            var tipoDoc = TipoComprobante.ConvertTipoFactura(sale_created.TypeDocumentSaleNavigation.TipoFactura);

            var ultimoComprobanteResponse = await _afipFacturacionService.GetUltimoComprobanteAutorizadoAsync(ptoVenta, tipoDoc);
            var nroFactura = ultimoComprobanteResponse.CbteNro + 1;

            var factura = new FacturaAFIP(sale_created, tipoDoc, nroFactura, ptoVenta, documentoAFacturar);
            var response = await _afipFacturacionService.FacturarAsync(factura);

            var facturaEmitida = FacturaExtension.ToFacturaEmitida(response.FECAEDetResponse.FirstOrDefault());
            facturaEmitida.IdSale = sale_created.IdSale;
            facturaEmitida.IdCliente = idCliente != 0 ? idCliente : null;
            facturaEmitida.RegistrationUser = registrationUser;
            facturaEmitida.NroFactura = string.IsNullOrEmpty(facturaEmitida.Errores) ? nroFactura : 0;
            facturaEmitida.PuntoVenta = ptoVenta;
            facturaEmitida.IdTienda = sale_created.IdTienda;
            facturaEmitida.TipoFactura = tipoDoc.Description;
            facturaEmitida.ImporteTotal = (decimal)factura.Detalle.First().ImporteTotal;
            facturaEmitida.ImporteNeto = (decimal)factura.Detalle.First().ImporteNeto;
            facturaEmitida.ImporteIVA = (decimal)factura.Detalle.First().ImporteIVA;

            var facturaEmitidaResponse = await _repository.Add(facturaEmitida);
            return facturaEmitidaResponse;
        }

        public async Task<List<FacturaEmitida>> GetAll(int idTienda)
        {
            var facturas = await _repository.Query(x => x.IdTienda == idTienda);
            return await facturas.Include(_ => _.Sale).Include(_ => _.Sale.TypeDocumentSaleNavigation).ToListAsync();
        }

        public async Task<FacturaEmitida> GetById(int idFacturaEmitida)
        {
            var facturas = await _repository.Query(x => x.IdFacturaEmitida == idFacturaEmitida);
            return facturas.Include(_ => _.Sale).Include(_ => _.Sale.TypeDocumentSaleNavigation).FirstOrDefault();
        }

        public async Task<string> GenerateFacturaQR(FacturaEmitida factura)
        {
            var ajustes = await _ajusteService.GetAjustesFacturacion(factura.IdTienda);

            var tipoComprobante = TipoComprobante.GetByDescription(factura.TipoFactura);

            var facturaQR = new FacturaQR
            {
                ver = 1,
                fecha = factura.FechaEmicion.ToString("yyyy-MM-dd"),
                cuit = ajustes.Cuit.Value,
                ptoVta = factura.PuntoVenta,
                tipoCmp = tipoComprobante.Id,
                nroCmp = factura.NroFactura.GetValueOrDefault(),
                importe = factura.ImporteTotal.GetValueOrDefault(),
                moneda = "PES",
                ctz = 1,
                tipoDocRec = factura.TipoDocumentoId,
                nroDocRec = factura.NroDocumento,
                tipoCodAut = "E", // EE es CAE y  A es CAEA
                codAut = Convert.ToInt64(factura.CAE)
            };

            var json = JsonSerializer.Serialize(facturaQR);
            var base64Json = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            var qrUrl = $"{URL_AFIP_QR}{base64Json}";

            return qrUrl;
        }

        public async Task<string> ReplaceCertificateAsync(IFormFile file, int idTienda)
        {
            return await _fileStorageService.ReplaceCertificateAsync(file, idTienda);
        }


        public VMX509Certificate2 GetCertificateAfipInformation(string certificatePath, string certificatePassword)
        {
            return _fileStorageService.GetCertificateAfipInformation(certificatePath, certificatePassword);
        }
    }
}
