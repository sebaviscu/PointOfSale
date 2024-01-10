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
            return query.OrderBy(_ => _.Nombre).ToList();
        }

        public async Task<Tienda> Add(Tienda entity)
        {
            try
            {
                var list = await List();

                if (list.Count() == 0)
                {
                    entity.Principal = true;
                }
                else if (list.Count() > 0 && entity.Principal == true)
                {
                    foreach (var l in list)
                    {
                        l.Principal= false;
                        await _repository.Edit(l);
                    }
                }

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
                Tienda_found.AumentoWeb = entity.AumentoWeb;
                Tienda_found.Direccion = entity.Direccion;
                Tienda_found.Telefono = entity.Telefono;
                Tienda_found.Email = entity.Email;
                Tienda_found.Logo = entity.Logo;
                Tienda_found.Whatsapp = entity.Whatsapp;
                Tienda_found.NombreImpresora = entity.NombreImpresora;
                Tienda_found.Facebook = entity.Facebook;
                Tienda_found.Instagram = entity.Instagram;
                Tienda_found.Twitter = entity.Twitter;
                Tienda_found.Tiktok = entity.Tiktok;
                Tienda_found.Youtube = entity.Youtube;
                Tienda_found.Lunes = entity.Lunes;
                Tienda_found.Martes = entity.Martes;
                Tienda_found.Miercoles = entity.Miercoles;
                Tienda_found.Jueves = entity.Jueves;
                Tienda_found.Viernes = entity.Viernes;
                Tienda_found.Sabado = entity.Sabado;
                Tienda_found.Domingo = entity.Domingo;
                Tienda_found.Feriado = entity.Feriado;
                Tienda_found.MontoEnvioGratis = entity.MontoEnvioGratis;
                Tienda_found.Principal = entity.Principal;
                Tienda_found.IdListaPrecio = entity.IdListaPrecio;

                Tienda_found.ModificationDate = DateTime.Now;
                Tienda_found.ModificationUser = entity.ModificationUser;

                var list = await List();

                if (list.Count() == 1)
                {
                    Tienda_found.Principal = true;
                }
                else if (list.Count() > 1 && entity.Principal == true)
                {
                    foreach (var l in list)
                    {
                        l.Principal = false;
                        await _repository.Edit(l);
                    }
                }

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
                    throw new TaskCanceledException("The Tienda no existe");


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
                    throw new TaskCanceledException("Tienda no se pudo encontrar.");

                return Tienda_found;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Tienda> GetTiendaPrincipal()
        {
            try
            {
                Tienda Tienda_found = await _repository.First(_ => _.Principal == true);

                if (Tienda_found == null)
                    throw new TaskCanceledException("Tienda no se pudo encontrar.");

                return Tienda_found;
            }
            catch
            {
                throw;
            }
        }
    }
}
