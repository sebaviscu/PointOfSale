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

namespace PointOfSale.Business.Services
{
    public class AfipService : IAfipService
    {
        const int ptoVenta = 1;
        const int defaultDocumento = 12345678;

        private readonly IGenericRepository<FacturaEmitida> _repository;
        private readonly IAFIPFacturacionService _afipFacturacionService;

        public AfipService(IGenericRepository<FacturaEmitida> repository, IAFIPFacturacionService afipFacturacionService)
        {
            _repository = repository;
            _afipFacturacionService = afipFacturacionService;
        }

        public async Task<FacturaEmitida> Facturar(Sale sale_created, int? nroDocumento, int? idCliente, string registrationUser)
        {
            if(sale_created.TypeDocumentSaleNavigation == null)
            {
                throw new Exception("El tipo de documento no existe");
            }

            if (nroDocumento == null)
                nroDocumento = defaultDocumento;

            var tipoDoc = TipoComprobante.ConvertTipoFactura(sale_created.TypeDocumentSaleNavigation.TipoFactura);

            var ultimoComprobanteResponse = await _afipFacturacionService.GetUltimoComprobanteAutorizadoAsync(ptoVenta, tipoDoc);

            var factura = new FacturaAFIP(sale_created, tipoDoc, ultimoComprobanteResponse.CbteNro + 1, ptoVenta, nroDocumento.Value);
            var response = await _afipFacturacionService.FacturarAsync(factura);

            var facturaEmitida = FacturaExtension.ToFacturaEmitida(response.FECAEDetResponse.FirstOrDefault());
            facturaEmitida.IdSale = sale_created.IdSale;
            facturaEmitida.IdCliente = idCliente != 0 ? idCliente : null;
            facturaEmitida.RegistrationUser = registrationUser;

            var facturaEmitidaResponse = await _repository.Add(facturaEmitida);
            return facturaEmitidaResponse;
        }
    }
}
 