using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface IProveedorService
    {
        Task<List<Proveedor>> List();
        Task<Proveedor> Add(Proveedor entity);
        Task<Proveedor> Edit(Proveedor entity);
        Task<bool> Delete(int idUser);
        Task<ProveedorMovimiento> Add(ProveedorMovimiento entity);
        Task<List<ProveedorMovimiento>> ListMovimientosProveedor(int idProveedor, int idTienda);
        Task<List<ProveedorMovimiento>> ListMovimientosProveedorForTablaDinamica(int idTienda);
        Task<ProveedorMovimiento> CambiarEstadoMovimiento(int idMovimiento);
        Task<List<Proveedor>> ListConProductos();
        Task<bool> DeleteProveedorMovimiento(int idProveedorMovimiento);
        Task<ProveedorMovimiento> Edit(ProveedorMovimiento entity);
    }
}
