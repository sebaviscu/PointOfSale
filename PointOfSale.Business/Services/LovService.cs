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
    public class LovService : ILovService
    {
        private readonly IGenericRepository<Lov> _repository;


        public async Task<List<Lov>> GetLovByType(LovType lovType)
        {
            var query = await _repository.Query(_=>_.LovType == lovType);
            return query.ToList();
        }

        public async Task<Lov> Add(Lov entity)
        {
            var Lov_created = await _repository.Add(entity);
            if (Lov_created.IdLov == 0)
                throw new TaskCanceledException("Lov no se pudo crear.");

            return Lov_created;
        }

        public async Task<Lov> Edit(Lov entity)
        {
            var Lov_edit = await _repository.First(_ => _.IdLov == entity.IdLov);

            if (Lov_edit == null)
                throw new TaskCanceledException("El Lov no existe");

            Lov_edit.Descripcion = entity.Descripcion;
            Lov_edit.Estado = entity.Estado;

            await _repository.EditAsync(Lov_edit);
            await _repository.SaveChangesAsync();

            return Lov_edit;
        }

        public async Task<bool> Delete(int idLov)
        {
            var entity = await _repository.Get(c => c.IdLov == idLov);

            if (entity == null)
                throw new TaskCanceledException("El Lov no existe");

            return await _repository.Delete(entity);
        }


    }
}
