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

namespace PointOfSale.Business.Services
{
    public class MovimientoCajaService : IMovimientoCajaService
    {
        private readonly IGenericRepository<MovimientoCaja> _repository;
        private readonly IGenericRepository<RazonMovimientoCaja> _repositoryRazonMovimientoCaja;

        public MovimientoCajaService(IGenericRepository<MovimientoCaja> repository, IGenericRepository<RazonMovimientoCaja> repositoryRazonMovimientoCaja)
        {
            _repository = repository;
            _repositoryRazonMovimientoCaja = repositoryRazonMovimientoCaja;
        }

        public async Task<List<RazonMovimientoCaja>> GetRazonMovimientoCaja()
        {
            var query = await _repositoryRazonMovimientoCaja.Query(_=>_.Estado);

            return await query.ToListAsync();
        }

        public async Task<List<MovimientoCaja>> List(int idTienda)
        {
            var query = await _repository.Query(_ => _.IdTienda == idTienda);

            return await query.Include(_ => _.RazonMovimientoCaja).ToListAsync();
        }

        public async Task<bool> UpdateEstado(int idEntity)
        {
            var razon = await _repositoryRazonMovimientoCaja.First(_ => _.IdRazonMovimientoCaja == idEntity);
            if (razon == null)
                throw new TaskCanceledException("Razon Movimiento Caja no se ha encontrado.");

            razon.Estado = !razon.Estado;

            var response = await _repositoryRazonMovimientoCaja.Edit(razon);

            if (!response)
                throw new TaskCanceledException("Razon Movimiento Caja no se pudo cambiar.");

            return response;
        }

        public async Task<RazonMovimientoCaja> Add(RazonMovimientoCaja entity)
        {
            var response = await _repositoryRazonMovimientoCaja.Add(entity);

            if (response.IdRazonMovimientoCaja == 0)
                throw new TaskCanceledException("Razon MovimientoCaja Caja no se pudo crear.");

            return response;
        }

        public async Task<MovimientoCaja> Add(MovimientoCaja entity)
        {
            if (string.IsNullOrEmpty(entity.Comentario))
            {
                throw new Exception("No es posible crear un movimiento de caja sin un comentario");
            }

            var response = await _repository.Add(entity);

            if (response.IdMovimientoCaja == 0)
                throw new TaskCanceledException("Movimiento Caja no se pudo crear.");

            return response;
        }

        public async Task<List<MovimientoCaja?>> GetMovimientoCajaByTurno(int idTurno)
        {
            var res = await _repository.Query(_ => _.IdTurno == idTurno);
            return res.Include(_=>_.RazonMovimientoCaja).ToList();
        }
    }
}
