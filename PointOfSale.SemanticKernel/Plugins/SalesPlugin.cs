using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using PointOfSale.Data.DBContext;
using PointOfSale.Data.Repository;
using PointOfSale.Model;

namespace PointOfSale.SemanticKernel.Plugins
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
        [Description("Gets a list of sales from the creation date passed by parameter")]
        [return: Description("An array of sales")]
        public async Task<List<Sale>> GetSalesFromCreationDateAsync(DateTime creationDate)
        {
            var list = await _genericRepository.Query(_=>_.RegistrationDate >= creationDate);
            return list.ToList();
        }
    }
}
