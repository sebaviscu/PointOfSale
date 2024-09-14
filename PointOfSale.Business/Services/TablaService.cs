using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Services
{
    public class TablaService : ITablaService
    {

        private readonly IGenericRepository<FormatosVenta> _repositoryFormatosVenta;

        public TablaService(IGenericRepository<FormatosVenta> repositoryFormatosVenta)
        {
            _repositoryFormatosVenta = repositoryFormatosVenta;
        }

        public async Task<List<FormatosVenta>> ListFormatosVenta()
        {
            var query = await _repositoryFormatosVenta.Query();
            return query.OrderBy(_ => _.Valor).ToList();
        }

        public async Task<List<FormatosVenta>> ListFormatosVentaActive()
        {
            var query = await _repositoryFormatosVenta.Query(_ => _.Estado);
            return query.OrderBy(_ => _.Valor).ToList();
        }

        public async Task<FormatosVenta> Add(FormatosVenta entity)
        {
            var FormatosVenta_created = await _repositoryFormatosVenta.Add(entity);
            if (FormatosVenta_created.IdFormatosVenta == 0)
                throw new TaskCanceledException("Formatos Venta no se pudo crear.");

            return FormatosVenta_created;
        }

        public async Task<FormatosVenta> Edit(FormatosVenta entity)
        {
            var FormatosVenta_found = await _repositoryFormatosVenta.Get(c => c.IdFormatosVenta == entity.IdFormatosVenta);

            FormatosVenta_found.Valor = entity.Valor;
            FormatosVenta_found.Formato = entity.Formato;
            FormatosVenta_found.Estado = entity.Estado;

            bool response = await _repositoryFormatosVenta.Edit(FormatosVenta_found);

            if (!response)
                throw new TaskCanceledException("Formatos Venta no se pudo cambiar.");

            return FormatosVenta_found;
        }

        public async Task<bool> Delete(int idFormatosVenta)
        {
            var FormatosVenta_found = await _repositoryFormatosVenta.Get(c => c.IdFormatosVenta == idFormatosVenta);

            if (FormatosVenta_found == null)
                throw new TaskCanceledException("El Formatos Venta no existe");

            bool response = await _repositoryFormatosVenta.Delete(FormatosVenta_found);

            return response;
        }
    }
}
