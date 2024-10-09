using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Services
{
    public class LovService : ServiceBase<Lov>, ILovService
    {

        public LovService(IGenericRepository<Lov> genericRepository) : base(genericRepository)
        {
        }

        public async Task<List<Lov>> GetLovByType(LovType lovType)
        {
            var list = base.List().Where(_ => _.LovType == lovType);
            return await list.ToListAsync();
        }

        public async Task<List<Lov>> GetLovActiveByType(LovType lovType)
        {
            var listActive = base.ListActive().Where(_ => _.LovType == lovType);
            return await listActive.ToListAsync();
        }

        public async Task<Lov?> GetById(int id)
        {
            return await _repository.First(_=>_.Id == id);
        }

        public async Task<Lov> Add(Lov entity)
        {
            var Lov_created = await base.Add(entity);

            return Lov_created;
        }

        public async Task<Lov> Edit(Lov entity)
        {
            var Lov_edit = await base.Edit(entity);

            return Lov_edit;
        }

        public async Task<bool> Delete(int id)
        {
            return await base.Delete(id);
        }

    }
}
