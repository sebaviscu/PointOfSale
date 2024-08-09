using PointOfSale.Model;
using System.Security.Cryptography.X509Certificates;

namespace PointOfSale.Business.Contracts
{
	public interface ITiendaService
	{
		Task<List<Tienda>> List();
		Task<Tienda> Add(Tienda entity);
		Task<Tienda> Edit(Tienda entity);
		Task<bool> Delete(int idTienda);
		Task<Tienda> Get(int tiendaId);
		X509Certificate2 GetCertificateAfipInformation(string certificatePath, string certificatePassword);
		Task<Tienda> EditCertificate(int idTienda, string certificadoNombre);
		Task<Tienda> GetWithPassword(int tiendaId);
    }
}
