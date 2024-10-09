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
    public class PagoEmpresaService : ServiceBase<PagoEmpresa>, IPagoEmpresaService
    {
        private readonly IGenericRepository<Empresa> _empresaRepository;
        public PagoEmpresaService(IGenericRepository<PagoEmpresa> genericRepository, IGenericRepository<Empresa> empresaRepository) : base(genericRepository)
        {
            _empresaRepository = empresaRepository;
        }

        public async Task<Empresa> GetEmpresa()
        {
            return await _empresaRepository.First();
        }
        public async Task<bool> UpdateEmpresa(Empresa empresa)
        {
            var e = await GetEmpresa();
            e.RazonSocial = empresa.RazonSocial;
            e.Comentario = empresa.Comentario;
            e.FrecuenciaPago = empresa.FrecuenciaPago;
            e.NombreContacto = empresa.NombreContacto;
            e.NumeroContacto = empresa.NumeroContacto;
            e.ProximoPago = empresa.ProximoPago;
            e.Licencia = empresa.Licencia;
            e.ModificationDate = empresa.ModificationDate;
            e.ModificationUser = empresa.ModificationUser;

            return await _empresaRepository.Edit(e);
            
        }
    }
}
