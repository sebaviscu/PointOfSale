using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Services
{
	public class TiendaService : ITiendaService
	{
		private readonly IGenericRepository<Tienda> _repository;
		public TiendaService(IGenericRepository<Tienda> repository)
		{
			_repository = repository;
		}

		public async Task<List<Tienda>> List()
		{

			IQueryable<Tienda> query = await _repository.Query();
			return query.ToList();
		}

		public async Task<Tienda> Add(Tienda entity)
		{
			try
			{
				Tienda Tienda_created = await _repository.Add(entity);
				if (Tienda_created.IdTienda == 0)
					throw new TaskCanceledException("Tienda no se pudo crear.");

				return Tienda_created;
			}
			catch
			{
				throw;
			}
		}

		public async Task<Tienda> Edit(Tienda entity)
		{
			try
			{
				Tienda Tienda_found = await _repository.Get(c => c.IdTienda == entity.IdTienda);

				Tienda_found.Nombre = entity.Nombre;
				Tienda_found.ModificationDate = DateTime.Now;
				Tienda_found.ModificationUser = entity.ModificationUser;

				bool response = await _repository.Edit(Tienda_found);

				if (!response)
					throw new TaskCanceledException("Tienda no se pudo cambiar.");

				return Tienda_found;
			}
			catch
			{
				throw;
			}
		}

		public async Task<bool> Delete(int idTienda)
		{
			try
			{
				Tienda Tienda_found = await _repository.Get(c => c.IdTienda == idTienda);

				if (Tienda_found == null)
					throw new TaskCanceledException("The Tienda does not exist");


				bool response = await _repository.Delete(Tienda_found);

				return response;
			}
			catch (Exception ex)
			{
				throw;
			}
		}


        public async Task<Tienda> Get(int tiendaId)
        {
            try
            {
                Tienda Tienda_found = await _repository.Get(c => c.IdTienda == tiendaId);

                if (Tienda_found == null)
                    throw new TaskCanceledException("Tienda no se pudo cambiar.");

                return Tienda_found;
            }
            catch
            {
                throw;
            }
        }

    }
}
