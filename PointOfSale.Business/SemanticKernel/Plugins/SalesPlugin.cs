using System.ComponentModel;
using Microsoft.SemanticKernel;
using PointOfSale.Data.DBContext;
using PointOfSale.Data.Repository;
using PointOfSale.Model;


namespace PointOfSale.Business.Plugins
{
    public class SalesPlugin
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Sale> _genericRepository;
        public SalesPlugin(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _genericRepository = _unitOfWork.Repository<Sale>();
        }

        [KernelFunction("get_sales")]
        [Description("Obtiene una lista de ventas desde la fecha de creación")]
        [return: Description("Lista de ventas")]
        public async Task<List<Sale>> GetSalesFromCreationDateAsync([Description("Fecha de creacion de la venta desde")] DateTime creationDate)
        {
            var list = await _genericRepository.Query(_ => _.RegistrationDate >= creationDate);
            return list.ToList();
        }

        [KernelFunction("get_total_sales")]
        [Description("Devuelve el total de la suma de ventas por su fecha de creacion")]
        [return: Description("Total de ventas")]
        public async Task<decimal?> GetSummatyTotalSales([Description("Fecha de creacion de la venta desde")] DateTime creationDateFrom, [Description("Fecha de creacion de la venta hasta")] DateTime? creationDateTo)
        {
            var list = await _genericRepository.Query(_ => _.RegistrationDate >= creationDateFrom);

            if (creationDateTo != null)
                list = list.Where(_ => _.RegistrationDate <= creationDateTo);

            return list.Sum(_ => _.Total);
        }
    }
}
