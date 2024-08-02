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

namespace PointOfSale.Business.Services
{
    public class AfipService : IAfipService
    {
        const int ptoVenta = 1;

        private readonly IGenericRepository<Gastos> _repository;
        private readonly IAFIPFacturacionService _afipFacturacionService;

        public AfipService(IGenericRepository<Gastos> repository, IAFIPFacturacionService afipFacturacionService)
        {
            _repository = repository;
            _afipFacturacionService = afipFacturacionService;
        }

        public async Task<bool> Facturar(Sale sale_created, int documento)
        {
            var tipoDoc = TipoComprobante.Factura_C;

            var ultimoComprobanteResponse = await _afipFacturacionService.GetUltimoComprobanteAutorizadoAsync(ptoVenta, tipoDoc);

            var factura = new FacturaAFIP(sale_created, tipoDoc, ultimoComprobanteResponse.CbteNro + 1, ptoVenta, documento);
            var response = await _afipFacturacionService.FacturarAsync(factura);

            FacturaExtension.ToFacturaEmitida(response.FECAECabResponse);
            return true;
        }
    }
}
