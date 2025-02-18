using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System.Text;
using AFIP.Facturacion.Model;
using PointOfSale.Model.Afip.Factura;
using AFIP.Facturacion.Services;
using PointOfSale.Business.Contracts;
using AFIP.Facturacion.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static PointOfSale.Model.Enum;
using Microsoft.AspNetCore.Http;
using PointOfSale.Business.Utilities;

namespace PointOfSale.Business.Services
{
    public class AfipService : IAfipService
    {
        const long defaultDocumento = 0;
        const string URL_AFIP_QR = "https://www.afip.gob.ar/fe/qr/?p=";

        private readonly IGenericRepository<FacturaEmitida> _repository;
        //private readonly IAFIPFacturacionService _afipFacturacionService;
        private readonly IAjusteService _ajusteService;
        private readonly IFileStorageService _fileStorageService;
        private readonly INotificationService _notificationRepository;
        private readonly ISaleRepository _saleRepository;
        private readonly IPrintService _printService;

        public AfipService(IGenericRepository<FacturaEmitida> repository,
            //IAFIPFacturacionService afipFacturacionService,
            IAjusteService ajusteService,
            IFileStorageService fileStorageService,
            INotificationService notificationRepository,
            ISaleRepository saleRepository,
            IPrintService printService)
        {
            _repository = repository;
            //_afipFacturacionService = afipFacturacionService;
            _ajusteService = ajusteService;
            _fileStorageService = fileStorageService;
            _notificationRepository = notificationRepository;
            _saleRepository = saleRepository;
            _printService = printService;
        }

        private static TipoComprobante ObtenerTipoNotaCredito(FacturaEmitida facturaEmitida)
        {
            var tipoDoc = TipoComprobante.NotaCredito_B;

            switch (facturaEmitida.TipoFactura)
            {
                case "Factura A":
                    tipoDoc = TipoComprobante.NotaCredito_A;
                    break;
                case "Factura B":
                    tipoDoc = TipoComprobante.NotaCredito_B;
                    break;
                case "Factura C":
                    tipoDoc = TipoComprobante.NotaCredito_C;
                    break;
            }

            return tipoDoc;
        }

        private TipoFactura ObtenerTipoFactura(TipoFactura tipoFactura, string? nroDocumento)
        {
            if (tipoFactura == TipoFactura.A && nroDocumento == null)
            {
                throw new Exception("La Factura A tiene que tener un número de documento");
            }
            return tipoFactura;
        }

        private long ObtenerDocumentoAFacturar(TipoComprobante tipoFactura, string? nroDocumento)
        {
            return (tipoFactura.Id == TipoComprobante.Factura_A.Id || tipoFactura.Id == TipoComprobante.Factura_B.Id) && nroDocumento != null
                ? Convert.ToInt64(nroDocumento)
                : defaultDocumento;
        }

        private FacturaEmitida CrearFacturaEmitida(int? idCliente, string registrationUser, DateTime registrationDate, FacturaAFIP factura, int idTienda, int idSale)
        {
            var facturaEmitida = new FacturaEmitida();

            facturaEmitida.IdSale = idSale;
            facturaEmitida.IdCliente = idCliente != 0 ? idCliente : null;
            facturaEmitida.RegistrationUser = registrationUser;
            facturaEmitida.PuntoVenta = factura.Cabecera.PuntoVenta;
            facturaEmitida.IdTienda = idTienda;
            facturaEmitida.TipoFactura = factura.Cabecera.TipoComprobante.Description;
            facturaEmitida.ImporteTotal = (decimal)factura.ImportesIva.Sum(_ => _.ImporteTotal);
            facturaEmitida.ImporteNeto = (decimal)factura.ImportesIva.Sum(_ => _.ImporteNeto);
            facturaEmitida.ImporteIVA = (decimal)factura.ImportesIva.Sum(_ => _.ImporteIVA);
            facturaEmitida.RegistrationDate = registrationDate;
            facturaEmitida.DetalleFacturaIvas = new List<DetalleFacturaIva>();
            foreach (var i in factura.ImportesIva)
            {
                facturaEmitida.DetalleFacturaIvas.Add(new DetalleFacturaIva(i));
            }
            return facturaEmitida;
        }

        public async Task<List<FacturaEmitida>> GetAll(int idTienda)
        {
            var facturas = await _repository.Query(x => x.IdTienda == idTienda);
            return await facturas
                .Include(_ => _.Sale.TypeDocumentSaleNavigation)
                .Include(_ => _.DetalleFacturaIvas)
                .Include(_ => _.Sale)
                    .ThenInclude(_ => _.DetailSales)
                .ToListAsync();
        }

        public async Task<List<FacturaEmitida>> GetAllTakeLimit(int idTienda, int limit = 500)
        {
            var facturas = await _repository.Query(x => x.IdTienda == idTienda);
            return await facturas
                .Include(_ => _.Sale.TypeDocumentSaleNavigation)
                .Include(_ => _.DetalleFacturaIvas)
                .Include(_ => _.Sale)
                    .ThenInclude(_ => _.DetailSales)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<FacturaEmitida> GetById(int idFacturaEmitida)
        {
            var facturas = await _repository.Query(x => x.IdFacturaEmitida == idFacturaEmitida);
            return facturas
                .Include(_ => _.Sale.TypeDocumentSaleNavigation)
                .Include(_ => _.DetalleFacturaIvas)
                .Include(_ => _.Sale)
                    .ThenInclude(_ => _.DetailSales)
                .FirstOrDefault();
        }

        public async Task<FacturaEmitida> GetBySaleId(int idSale)
        {
            var facturas = await _repository.Query(x => x.IdSale == idSale);
            return facturas.FirstOrDefault();
        }

        public async Task<string> GenerateLinkAfipFactura(FacturaEmitida factura)
        {
            if (!factura.NroFactura.HasValue)
            {
                return null;
            }

            var ajustes = await _ajusteService.GetAjustesFacturacion(factura.IdTienda);

            var tipoComprobante = TipoComprobante.GetByDescription(factura.TipoFactura);

            var facturaQR = new FacturaQR
            {
                ver = 1,
                fecha = factura.FechaEmicion.Value.ToString("yyyy-MM-dd"),
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

        public async Task<string> ReplaceCertificateAsync(IFormFile file, int idTienda, string? oldCertificateName)
        {
            return await _fileStorageService.ReplaceCertificateAsync(file, idTienda, oldCertificateName);
        }

        public VMX509Certificate2 GetCertificateAfipInformation(string certificatePath, string certificatePassword)
        {
            return _fileStorageService.GetCertificateAfipInformation(certificatePath, certificatePassword);
        }

        public async Task CheckVencimientoCertificado(int idTienda)
        {
            var ajustes = await _ajusteService.GetAjustesFacturacion(idTienda);
            if (ajustes != null)
            {
                if (ajustes.CertificadoFechaCaducidad.HasValue && ajustes.CertificadoFechaCaducidad.Value.Date <= DateTime.Now.Date.AddDays(15))
                {
                    var notific = new Notifications(ajustes);
                    await _notificationRepository.Save(notific);

                }
            }
        }

        public async Task<List<FacturaAFIP>> GetFacturaByVentas(List<Sale> sales, Ajustes ajustes, string cuil, int? idCliente)
        {
            if (!ajustes.FacturaElectronica.HasValue || !ajustes.FacturaElectronica.Value)
            {
                return default;
            }

            var idTienda = sales.First().IdTienda;
            var facturasList = new List<FacturaAFIP>();
            AjustesFacturacion ajustesFacturacion;
            var fechaFactura = TimeHelper.GetArgentinaTime();

            if (sales.Any(_ => _.TipoFactura.HasValue && (int)_.TipoFactura < 3))
            {
                ajustesFacturacion = await _ajusteService.GetAjustesFacturacion(idTienda);

                foreach (var s in sales)
                {
                    if (s.TipoFactura.HasValue && (int)s.TipoFactura < 3)
                    {
                        var tipoFactura = ObtenerTipoFactura(s.TipoFactura.Value, cuil);
                        var tipoDoc = TipoComprobante.ConvertTipoFactura(tipoFactura);
                        var documentoAFacturar = ObtenerDocumentoAFacturar(tipoDoc, cuil);

                        var factura = new FacturaAFIP(s.DetailSales.ToList(), s.RegistrationDate.Value, tipoDoc, nroComprobante: null, ajustesFacturacion.PuntoVenta.Value, documentoAFacturar);

                        var facturaEmitida = CrearFacturaEmitida(idCliente, s.RegistrationUser, fechaFactura, factura, idTienda, s.IdSale);
                        var factCreada = await _repository.Add(facturaEmitida);

                        factura.IdFacturaEmitida = factCreada.IdFacturaEmitida;
                        facturasList.Add(factura);
                    }
                }
            }

            return facturasList;
        }

        public async Task<FacturaEmitida> SaveFacturaEmitida(FacturacionResponse facturacion, int idFacturaEmitida)
        {
            var facturas = await GetById(idFacturaEmitida);

            FacturaExtension.ToFacturaEmitida(facturacion.FECAEDetResponse.First(), facturas);
            await _repository.Edit(facturas);

            return facturas;
        }

        public async Task<FacturaEmitida> EditeFacturaError(int idFacturaEmitida, string error)
        {
            var facturas = await GetById(idFacturaEmitida);

            facturas.Resultado = "F";
            facturas.Observaciones = error;
            if (!string.IsNullOrEmpty(facturas.CAE))
            {
                facturas.Observaciones += $" - - SE HABIA GENERADO CAE:{facturas.CAE} + vencimiento: {facturas.CAEVencimiento}";
                facturas.CAE = string.Empty;
                facturas.CAEVencimiento = null;
            }
            await _repository.Edit(facturas);

            return facturas;
        }

        public async Task<FacturaAFIP?> NotaCredito(int idFacturaemitida, string registrationUser)
        {
            var facturaOriginal = await GetById(idFacturaemitida);

            var ajustesFacturacion = await _ajusteService.GetAjustesFacturacion(facturaOriginal.IdTienda);

            var tipoDoc = ObtenerTipoNotaCredito(facturaOriginal);

            var factura = new FacturaAFIP(facturaOriginal.Sale.DetailSales.ToList(), tipoDoc, nroComprobante: null, facturaOriginal, facturaOriginal.NroDocumento.Value, isNotaCredito: true);

            var notaCredito = CrearFacturaEmitida(facturaOriginal.IdCliente, registrationUser, TimeHelper.GetArgentinaTime(), factura, facturaOriginal.IdTienda, facturaOriginal.IdSale.Value);
            notaCredito.IdFacturaAnulada = facturaOriginal.IdFacturaEmitida;
            notaCredito.FacturaAnulada = $"{facturaOriginal.TipoFactura} {facturaOriginal.NumeroFacturaString}";

            facturaOriginal.Observaciones = "Factura cancelada con notade credito";
            facturaOriginal.FacturaRefacturada = "Anulada";
            await _repository.Edit(facturaOriginal);

            var notaCreditoEmitida = await _repository.Add(notaCredito);
            factura.IdFacturaEmitida = notaCreditoEmitida.IdFacturaEmitida;
            return factura;
        }

        public async Task<FacturaAFIP?> Refacturar(int idFacturaemitida, string? cuil, string registrationUser)
        {
            var facturaEmitida = await GetById(idFacturaemitida);

            var ajustesFacturacion = await _ajusteService.GetAjustesFacturacion(facturaEmitida.IdTienda);

            var tipoDoc = TipoComprobante.GetByDescription(facturaEmitida.TipoFactura);
            var documentoAFacturar = ObtenerDocumentoAFacturar(tipoDoc, cuil);

            var factura = new FacturaAFIP(facturaEmitida.Sale.DetailSales.ToList(), tipoDoc, null, facturaEmitida, documentoAFacturar, isNotaCredito: false);

            facturaEmitida.FacturaRefacturada = "Refacturada";
            await _repository.Edit(facturaEmitida);

            var sale = await _saleRepository.First(_ => _.IdSale == facturaEmitida.IdSale);
            sale.Observaciones = "Factura refacturada";

            _ = await _saleRepository.Edit(sale);
            return factura;
        }

    }
}
