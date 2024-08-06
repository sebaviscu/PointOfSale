using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface IAfipService
    {
        Task<FacturaEmitida> Facturar(Sale sale_created, int? nroDocumento, int? idCliente, string registrationUser);

        Task<List<FacturaEmitida>> GetAll(int idTienda);
    }
}
