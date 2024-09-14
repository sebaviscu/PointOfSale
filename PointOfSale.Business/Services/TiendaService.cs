using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.Repository;
using PointOfSale.Model;

namespace PointOfSale.Business.Services
{
    public class TiendaService : ITiendaService
    {
        private readonly IGenericRepository<Tienda> _repository;
        private readonly IGenericRepository<CorrelativeNumber> _correlativeNumber;
        private readonly IGenericRepository<Ajustes> _ajustes;
        private readonly IGenericRepository<AjustesFacturacion> _ajustesFacturacion;
        private readonly IGenericRepository<Turno> _turno;

        public TiendaService(IGenericRepository<Tienda> repository, IGenericRepository<CorrelativeNumber> correlativeNumber, IGenericRepository<Ajustes> ajustes, IGenericRepository<AjustesFacturacion> ajustesFacturacion, IGenericRepository<Turno> turno)
        {
            _repository = repository;
            _correlativeNumber = correlativeNumber;
            _ajustes = ajustes;
            _ajustesFacturacion = ajustesFacturacion;
            _turno = turno;
        }

        public async Task<List<Tienda>> List()
        {
            IQueryable<Tienda> query = await _repository.Query();
            return query.OrderBy(_ => _.Nombre).ToList();

        }

        public async Task<Tienda> Add(Tienda entity)
        {

            Tienda Tienda_created = await _repository.Add(entity);
            if (Tienda_created.IdTienda == 0)
                throw new TaskCanceledException("Tienda no se pudo crear.");

            return Tienda_created;
        }

        public async Task<Tienda> Edit(Tienda entity)
        {
            Tienda Tienda_found = await _repository.Get(c => c.IdTienda == entity.IdTienda);

            Tienda_found.Nombre = entity.Nombre;
            Tienda_found.Direccion = entity.Direccion;
            Tienda_found.Telefono = entity.Telefono;
            Tienda_found.Logo = entity.Logo;
            Tienda_found.IdListaPrecio = entity.IdListaPrecio;
            Tienda_found.Color = entity.Color;

            Tienda_found.ModificationDate = TimeHelper.GetArgentinaTime();
            Tienda_found.ModificationUser = entity.ModificationUser;

            bool response = await _repository.Edit(Tienda_found);

            if (!response)
                throw new TaskCanceledException("Tienda no se pudo cambiar.");

            return Tienda_found;
        }

        public async Task<bool> Delete(int idTienda)
        {
            // Incluir la entidad relacionada CorrelativeNumber
            var tienda = await _repository.First(t => t.IdTienda == idTienda);

            if (tienda == null)
            {
                throw new TaskCanceledException("La Tienda no existe");
            }

            var c = await _correlativeNumber.First(_ => _.IdTienda == idTienda);
            if (c != null)
            {
                await _correlativeNumber.Delete(c);
            }

            var a = await _ajustes.First(_ => _.IdTienda == idTienda);
            if (a != null)
            {
                await _ajustes.Delete(a);
            }

            var f = await _ajustesFacturacion.First(_ => _.IdTienda == idTienda);
            if (f != null)
            {
                await _ajustesFacturacion.Delete(f);
            }

            var t = await _turno.Query(_ => _.IdTienda == idTienda);
            foreach (var item in t)
            {
                await _turno.Delete(item);
            }

            return await _repository.Delete(tienda);
        }

        public async Task<Tienda> Get(int tiendaId)
        {
            Tienda Tienda_found = await _repository.Get(c => c.IdTienda == tiendaId);

            if (Tienda_found == null)
                throw new TaskCanceledException("Tienda no se pudo encontrar.");
            return Tienda_found;
        }
    }

}
