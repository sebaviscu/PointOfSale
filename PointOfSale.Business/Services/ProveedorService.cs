using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static iTextSharp.text.pdf.events.IndexEvents;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Services
{
    public class ProveedorService : IProveedorService
    {
        private readonly IGenericRepository<Proveedor> _repository;
        private readonly IGenericRepository<ProveedorMovimiento> _proveedorMovimiento;

        public ProveedorService(IGenericRepository<Proveedor> repository, IGenericRepository<ProveedorMovimiento> proveedorMovimiento)
        {
            _repository = repository;
            _proveedorMovimiento = proveedorMovimiento;
        }

        public async Task<List<Proveedor>> List()
        {
            IQueryable<Proveedor> query = await _repository.Query();
            return query.OrderBy(_ => _.Nombre).ToList();
        }
        public async Task<List<Proveedor>> ListConProductos(int idTienda)
        {
            IQueryable<Proveedor> query = await _repository.Query();
            return await query
                .Include(p => p.Products)
                    .ThenInclude(p => p.Stocks.Where(s => s.IdTienda == idTienda))
                .AsNoTracking()
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<Proveedor> Add(Proveedor entity)
        {
            Proveedor Proveedor_exists = await _repository.Get(u => u.Nombre == entity.Nombre);

            if (Proveedor_exists != null)
                throw new TaskCanceledException("El Proveedor ya existe");

            try
            {
                entity.RegistrationDate = TimeHelper.GetArgentinaTime();
                Proveedor Proveedor_created = await _repository.Add(entity);

                if (Proveedor_created.IdProveedor == 0)
                    throw new TaskCanceledException("Error al crear Proveedor");

                return Proveedor_created;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ProveedorMovimiento> Add(ProveedorMovimiento entity)
        {
            try
            {
                ProveedorMovimiento Proveedor_created = await _proveedorMovimiento.Add(entity);

                if (Proveedor_created.IdProveedorMovimiento == 0)
                    throw new TaskCanceledException("Error al crear Pago");

                IQueryable<ProveedorMovimiento> query = await _proveedorMovimiento.Query(u => u.IdProveedorMovimiento == entity.IdProveedorMovimiento);
                var Proveedor_edit = query.Include(_ => _.Proveedor).FirstOrDefault();

                return Proveedor_edit;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Proveedor> Edit(Proveedor entity)
        {

            try
            {
                IQueryable<Proveedor> queryProveedor = await _repository.Query(u => u.IdProveedor == entity.IdProveedor);

                Proveedor Proveedor_edit = queryProveedor.First();

                Proveedor_edit.Nombre = entity.Nombre;
                Proveedor_edit.Cuil = entity.Cuil;
                Proveedor_edit.Telefono = entity.Telefono;
                Proveedor_edit.Direccion = entity.Direccion;
                Proveedor_edit.ModificationDate = TimeHelper.GetArgentinaTime();
                Proveedor_edit.ModificationUser = entity.ModificationUser;

                Proveedor_edit.Telefono2 = entity.Telefono2;
                Proveedor_edit.Iva = entity.Iva;
                Proveedor_edit.TipoFactura = entity.TipoFactura;
                Proveedor_edit.Web = entity.Web;
                Proveedor_edit.Email = entity.Email;
                Proveedor_edit.Comentario = entity.Comentario;
                Proveedor_edit.NombreContacto = entity.NombreContacto;

                bool response = await _repository.Edit(Proveedor_edit);
                if (!response)
                    throw new TaskCanceledException("No se pudo modificar Proveedor");

                return Proveedor_edit;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ProveedorMovimiento> CambiarEstadoMovimiento(int idMovimiento)
        {
            try
            {
                IQueryable<ProveedorMovimiento> query = await _proveedorMovimiento.Query(u => u.IdProveedorMovimiento == idMovimiento);
                var Proveedor_edit = query.Include(_ => _.Proveedor).FirstOrDefault();

                if (Proveedor_edit == null)
                    throw new TaskCanceledException("Proveedor no existe");

                Proveedor_edit.EstadoPago = EstadoPago.Pagado;

                bool response = await _proveedorMovimiento.Edit(Proveedor_edit);
                if (!response)
                    throw new TaskCanceledException("No se pudo modificar el movimiento");

                return Proveedor_edit;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<ProveedorMovimiento> Edit(ProveedorMovimiento entity)
        {
            try
            {
                IQueryable<ProveedorMovimiento> query = await _proveedorMovimiento.Query(u => u.IdProveedorMovimiento == entity.IdProveedorMovimiento);
                var Proveedor_edit = query.Include(_ => _.Proveedor).FirstOrDefault();

                if (Proveedor_edit == null)
                    throw new TaskCanceledException("Proveedor no existe");

                Proveedor_edit.ModificationDate = TimeHelper.GetArgentinaTime();
                Proveedor_edit.Comentario = entity.Comentario;
                Proveedor_edit.EstadoPago = entity.EstadoPago;
                Proveedor_edit.FacturaPendiente = entity.FacturaPendiente;
                Proveedor_edit.Importe = entity.Importe;
                Proveedor_edit.ImporteSinIva = entity.ImporteSinIva;
                Proveedor_edit.Iva = entity.Iva;
                Proveedor_edit.IvaImporte = entity.IvaImporte;
                Proveedor_edit.NroFactura = entity.NroFactura;
                Proveedor_edit.TipoFactura = entity.TipoFactura;
                Proveedor_edit.IdProveedor = entity.IdProveedor;


                bool response = await _proveedorMovimiento.Edit(Proveedor_edit);
                if (!response)
                    throw new TaskCanceledException("No se pudo modificar el pago del proveedor");

                return Proveedor_edit;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteProveedorMovimiento(int idProveedorMovimiento)
        {
            try
            {
                ProveedorMovimiento Proveedor_found = await _proveedorMovimiento.Get(u => u.IdProveedorMovimiento == idProveedorMovimiento);

                if (Proveedor_found == null)
                    throw new TaskCanceledException("Proveedor no existe");

                bool response = await _proveedorMovimiento.Delete(Proveedor_found);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> Delete(int idProveedor)
        {
            try
            {
                Proveedor Proveedor_found = await _repository.Get(u => u.IdProveedor == idProveedor);

                if (Proveedor_found == null)
                    throw new TaskCanceledException("Proveedor no existe");

                bool response = await _repository.Delete(Proveedor_found);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<ProveedorMovimiento>> ListMovimientosProveedor(int idProveedor, int idTienda)
        {
            IQueryable<ProveedorMovimiento> query = await _proveedorMovimiento.Query(u => u.IdProveedor == idProveedor && u.idTienda == idTienda);

            return query.OrderByDescending(_ => _.RegistrationUser).ToList();
        }

        public async Task<List<ProveedorMovimiento>> ListMovimientosProveedorForTablaDinamica(int idTienda)
        {
            IQueryable<ProveedorMovimiento> query = await _proveedorMovimiento.Query(u =>u.idTienda == idTienda);

            return query.Include(_=>_.Proveedor).OrderByDescending(_ => _.RegistrationUser).ToList();
        }
    }
}
