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
    public class AjusteService : IAjusteService
    {
        public DateTime DateTimeNowArg => TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));
        private readonly IGenericRepository<Ajustes> _repository;

        public AjusteService(IGenericRepository<Ajustes> repository)
        {
            _repository = repository;
        }

        public async Task<Ajustes> Get()
        {
            return await _repository.First();

        }


        public async Task<Ajustes> Edit(Ajustes entity)
        {
            try
            {
                Ajustes Ajustes_found = await this.Get();

                Ajustes_found.ModificationDate = DateTimeNowArg;
                Ajustes_found.ModificationUser = entity.ModificationUser;

                Ajustes_found.Nombre = entity.Nombre;
                Ajustes_found.Direccion = entity.Direccion;
                Ajustes_found.AumentoWeb = entity.AumentoWeb;
                Ajustes_found.Whatsapp = entity.Whatsapp;
                Ajustes_found.Facebook = entity.Facebook;
                Ajustes_found.Instagram = entity.Instagram;
                Ajustes_found.Twitter = entity.Twitter;
                Ajustes_found.Tiktok = entity.Tiktok;
                Ajustes_found.Youtube = entity.Youtube;
                Ajustes_found.Lunes = entity.Lunes;
                Ajustes_found.Martes = entity.Martes;
                Ajustes_found.Miercoles = entity.Miercoles;
                Ajustes_found.Jueves = entity.Jueves;
                Ajustes_found.Viernes = entity.Viernes;
                Ajustes_found.Sabado = entity.Sabado;
                Ajustes_found.Domingo = entity.Domingo;
                Ajustes_found.Feriado = entity.Feriado;
                Ajustes_found.MontoEnvioGratis = entity.MontoEnvioGratis;

                Ajustes_found.CodigoSeguridad = entity.CodigoSeguridad;
                Ajustes_found.ImprimirDefault = entity.ImprimirDefault;

                bool response = await _repository.Edit(Ajustes_found);

                if (!response)
                    throw new TaskCanceledException("Ajustes no se pudo cambiar.");

                return Ajustes_found;
            }
            catch
            {
                throw;
            }
        }
    }
}
