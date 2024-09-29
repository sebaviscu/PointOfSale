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
using Microsoft.IdentityModel.Tokens;

namespace PointOfSale.Business.Services
{
    public class AfipService : IAfipService
    {
        const long defaultDocumento = 0;
        const string URL_AFIP_QR = "https://www.afip.gob.ar/fe/qr/?p=";

        private readonly IGenericRepository<FacturaEmitida> _repository;
        private readonly IAFIPFacturacionService _afipFacturacionService;
        private readonly IAjusteService _ajusteService;
        private readonly IFileStorageService _fileStorageService;
        private readonly INotificationService _notificationRepository;
        private readonly ISaleRepository _saleRepository;
        private readonly ITypeDocumentSaleService _typeDocumentSaleService;

        public AfipService(IGenericRepository<FacturaEmitida> repository,
            IAFIPFacturacionService afipFacturacionService,
            IAjusteService ajusteService,
            IFileStorageService fileStorageService,
            INotificationService notificationRepository,
            ISaleRepository saleRepository,
            ITypeDocumentSaleService typeDocumentSaleService)
        {
            _repository = repository;
            _afipFacturacionService = afipFacturacionService;
            _ajusteService = ajusteService;
            _fileStorageService = fileStorageService;
            _notificationRepository = notificationRepository;
            _saleRepository = saleRepository;
            _typeDocumentSaleService = typeDocumentSaleService;
        }

        private async Task<FacturaEmitida> Facturar(Sale sale, string? nroDocumento, int? idCliente, string registrationUser, AjustesFacturacion ajustes)
        {
            ValidarSale(sale);

            var validCert = ValidateCertificate(ajustes);
            if (validCert != string.Empty)
            {
                throw new Exception(validCert);
            }

            var tipoFactura = ObtenerTipoFactura(sale.TypeDocumentSaleNavigation.TipoFactura, nroDocumento);

            var tipoDoc = TipoComprobante.ConvertTipoFactura(tipoFactura);
            var documentoAFacturar = ObtenerDocumentoAFacturar(tipoDoc, nroDocumento);
            var nroFactura = await ObtenerNuevoNumeroFactura(ajustes, tipoDoc);

            var factura = new FacturaAFIP(sale.Total.Value, sale.RegistrationDate.Value, tipoDoc, nroFactura, ajustes.PuntoVenta.Value, documentoAFacturar);
            var response = await _afipFacturacionService.FacturarAsync(ajustes, factura);

            var facturaEmitida = CrearFacturaEmitida(response, nroFactura, idCliente, registrationUser, factura, sale.IdTienda, sale.IdSale);

            return await _repository.Add(facturaEmitida);
        }

        private async Task<FacturaEmitida> Refacturar(FacturaEmitida facturaEmitida, string? nroDocumento, int? idCliente, string registrationUser, AjustesFacturacion ajustes)
        {
            var validCert = ValidateCertificate(ajustes);
            if (validCert != string.Empty)
            {
                throw new Exception(validCert);
            }

            var tipoDoc = TipoComprobante.GetByDescription(facturaEmitida.TipoFactura);
            var documentoAFacturar = ObtenerDocumentoAFacturar(tipoDoc, nroDocumento);

            var nroFactura = await ObtenerNuevoNumeroFactura(ajustes, tipoDoc);

            var factura = new FacturaAFIP(tipoDoc, nroFactura, facturaEmitida, documentoAFacturar, isNotaCredito: false);
            var response = await _afipFacturacionService.FacturarAsync(ajustes, factura);

            var refactura = CrearFacturaEmitida(response, nroFactura, idCliente, registrationUser, factura, facturaEmitida.IdTienda, facturaEmitida.IdSale);

            return await _repository.Add(refactura);
        }

        private async Task<FacturaEmitida> Credito(string registrationUser, AjustesFacturacion ajustes, FacturaEmitida facturaEmitida)
        {
            var validCert = ValidateCertificate(ajustes);
            if (validCert != string.Empty)
            {
                throw new Exception(validCert);
            }

            var tipoDoc = ObtenerTipoNotaCredito(facturaEmitida);

            var nroFactura = await ObtenerNuevoNumeroFactura(ajustes, tipoDoc);

            var factura = new FacturaAFIP(tipoDoc, nroFactura, facturaEmitida, facturaEmitida.NroDocumento, isNotaCredito: true);
            var response = await _afipFacturacionService.FacturarAsync(ajustes, factura);

            var creditoEmitido = CrearFacturaEmitida(response, nroFactura, facturaEmitida.IdCliente, registrationUser, factura, facturaEmitida.IdTienda, null);
            creditoEmitido.IdFacturaAnulada = facturaEmitida.IdFacturaEmitida;
            creditoEmitido.FacturaAnulada = $"{facturaEmitida.TipoFactura} {facturaEmitida.NumeroFacturaString}";

            return await _repository.Add(creditoEmitido);
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

        private void ValidarSale(Sale sale)
        {
            if (sale.TypeDocumentSaleNavigation == null)
            {
                throw new Exception("El tipo de documento no existe");
            }
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

        private async Task<int> ObtenerNuevoNumeroFactura(AjustesFacturacion ajustes, TipoComprobante tipoDoc)
        {
            var ultimoComprobanteResponse = await _afipFacturacionService.GetUltimoComprobanteAutorizadoAsync(ajustes, ajustes.PuntoVenta.Value, tipoDoc);
            return ultimoComprobanteResponse.CbteNro + 1;
        }

        private FacturaEmitida CrearFacturaEmitida(FacturacionResponse response, int nroFactura, int? idCliente, string registrationUser, FacturaAFIP factura, int idTienda, int? idSale)
        {

            var facturaEmitida = FacturaExtension.ToFacturaEmitida(response.FECAEDetResponse.FirstOrDefault());

            facturaEmitida.IdSale = idSale;
            facturaEmitida.IdCliente = idCliente != 0 ? idCliente : null;
            facturaEmitida.RegistrationUser = registrationUser;
            facturaEmitida.NroFactura = string.IsNullOrEmpty(facturaEmitida.Observaciones) ? nroFactura : 0;
            facturaEmitida.PuntoVenta = factura.Cabecera.PuntoVenta;
            facturaEmitida.IdTienda = idTienda;
            facturaEmitida.TipoFactura = factura.Cabecera.TipoComprobante.Description;
            facturaEmitida.ImporteTotal = (decimal)factura.Detalle.First().ImporteTotal;
            facturaEmitida.ImporteNeto = (decimal)factura.Detalle.First().ImporteNeto;
            facturaEmitida.ImporteIVA = (decimal)factura.Detalle.First().ImporteIVA;

            return facturaEmitida;
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

        public async Task<FacturaEmitida> GetBySaleId(int idSale)
        {
            var facturas = await _repository.Query(x => x.IdSale == idSale);
            return facturas.FirstOrDefault();
        }

        public async Task<string> GenerateLinkAfipFactura(FacturaEmitida factura)
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

        public async Task<string> ReplaceCertificateAsync(IFormFile file, int idTienda, string? oldCertificateName)
        {
            return await _fileStorageService.ReplaceCertificateAsync(file, idTienda, oldCertificateName);
        }

        public VMX509Certificate2 GetCertificateAfipInformation(string certificatePath, string certificatePassword)
        {
            return _fileStorageService.GetCertificateAfipInformation(certificatePath, certificatePassword);
        }

        public string ValidateCertificate(AjustesFacturacion? ajustes)
        {
            var resp = string.Empty;
            try
            {
                if (ajustes == null || !ajustes.IsValid)
                {
                    throw new Exception("Los ajustes de facturación no existen o estan incorrectos");
                }

                var path = FileStorageService.ObtenerRutaCertificado(ajustes);
                FileStorageService.ExistCertificate(path);
            }
            catch (Exception e)
            {
                resp = e.Message;
            }

            return resp;
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


        public async Task<FacturaEmitida?> FacturarVenta(Sale sale, Ajustes ajustes, string cuil, int? idCliente)
        {
            if (!ajustes.FacturaElectronica.HasValue || !ajustes.FacturaElectronica.Value)
            {
                return null;
            }

            FacturaEmitida facturaEmitida = null;
            var tipoVenta = await _typeDocumentSaleService.Get(sale.IdTypeDocumentSale.Value);

            if ((int)tipoVenta.TipoFactura < 3)
            {
                var ajustesFacturacion = await _ajusteService.GetAjustesFacturacion(sale.IdTienda);

                sale.TypeDocumentSaleNavigation = tipoVenta;

                facturaEmitida = await Facturar(sale, cuil, idCliente, sale.RegistrationUser, ajustesFacturacion);


                sale.IdFacturaEmitida = facturaEmitida?.IdFacturaEmitida;
                sale.ResultadoFacturacion = facturaEmitida?.Resultado == "A";

                if (!sale.ResultadoFacturacion.Value)
                {
                    sale.Observaciones = $"Error en AFIP:\n{facturaEmitida.Observaciones}";
                }
                _ = await _saleRepository.Edit(sale);
            }

            return facturaEmitida;
        }

        public async Task<FacturaEmitida?> NotaCredito(int idFacturaemitida, string registrationUser)
        {
            var facturaEmitida = await _repository.First(_ => _.IdFacturaEmitida == idFacturaemitida);

            var ajustesFacturacion = await _ajusteService.GetAjustesFacturacion(facturaEmitida.IdTienda);

            return await Credito(registrationUser, ajustesFacturacion, facturaEmitida);
        }

        public async Task<FacturaEmitida?> Refacturar(int idFacturaemitida, string cuil, string registrationUser)
        {
            var facturaEmitida = await _repository.First(_ => _.IdFacturaEmitida == idFacturaemitida);

            var ajustesFacturacion = await _ajusteService.GetAjustesFacturacion(facturaEmitida.IdTienda);
            var refacturada = await Refacturar(facturaEmitida, cuil, facturaEmitida.IdCliente, registrationUser, ajustesFacturacion);

            facturaEmitida.FacturaRefacturada = refacturada.NumeroFacturaString;
            await _repository.Edit(facturaEmitida);

            var sale = await _saleRepository.First(_ => _.IdSale == facturaEmitida.IdSale.Value);

            sale.IdFacturaEmitida = refacturada?.IdFacturaEmitida;
            sale.ResultadoFacturacion = refacturada?.Resultado == "A";

            sale.Observaciones = !sale.ResultadoFacturacion.Value ? $"Error en AFIP:\n{refacturada.Observaciones}" : string.Empty;

            _ = await _saleRepository.Edit(sale);
            return refacturada;
        }

    }
}
