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
    public class AjusteService : IAjusteService
    {
        private readonly IGenericRepository<AjustesWeb> _repositoryWeb;
        private readonly IGenericRepository<Ajustes> _repositoryAjustes;

        public AjusteService(IGenericRepository<AjustesWeb> repository, IGenericRepository<Ajustes> repositoryAjustes)
        {
            _repositoryWeb = repository;
            _repositoryAjustes = repositoryAjustes;

        }

        public async Task<AjustesWeb> GetAjustesWeb()
        {
            return await _repositoryWeb.First();

        }


        public async Task<AjustesWeb> EditWeb(AjustesWeb entity)
        {
            AjustesWeb Ajustes_found = await this.GetAjustesWeb();

            Ajustes_found.ModificationDate = TimeHelper.GetArgentinaTime();
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

            bool response = await _repositoryWeb.Edit(Ajustes_found);

            if (!response)
                throw new TaskCanceledException("Ajustes Web no se pudo cambiar.");

            return Ajustes_found;
        }

        public async Task<Ajustes> GetAjustes(int idTienda)
        {
            return await _repositoryAjustes.First(_ => _.IdTienda == idTienda);
        }


        public async Task<Ajustes> Edit(Ajustes entity)
        {
            Ajustes Ajustes_found = await this.GetAjustes(entity.IdTienda);

            Ajustes_found.ModificationDate = TimeHelper.GetArgentinaTime();
            Ajustes_found.ModificationUser = entity.ModificationUser;

            Ajustes_found.CodigoSeguridad = entity.CodigoSeguridad;
            Ajustes_found.ImprimirDefault = entity.ImprimirDefault;
            Ajustes_found.NombreTiendaTicket = entity.NombreTiendaTicket;
            Ajustes_found.NombreImpresora = entity.NombreImpresora;
            Ajustes_found.MinimoIdentificarConsumidor = entity.MinimoIdentificarConsumidor;

            bool response = await _repositoryAjustes.Edit(Ajustes_found);

            if (!response)
                throw new TaskCanceledException("Ajustes no se pudo cambiar.");

            return Ajustes_found;

        }
    }
}
