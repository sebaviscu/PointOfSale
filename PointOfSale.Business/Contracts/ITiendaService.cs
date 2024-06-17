using PointOfSale.Model;

namespace PointOfSale.Business.Contracts
{
	public interface ITiendaService
	{
		Task<List<Tienda>> List();
		Task<Tienda> Add(Tienda entity);
		Task<Tienda> Edit(Tienda entity);
		Task<bool> Delete(int idTienda);
		Task<Tienda> Get(int tiendaId);

    }
}
