using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.DBContext;
using PointOfSale.Data.Repository;
using PointOfSale.Model;

namespace PointOfSale.Business.Services
{
    public class AjusteService : IAjusteService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IGenericRepository<Ajustes> _repositoryAjustes;
        private readonly IGenericRepository<AjustesFacturacion> _repositoryAjustesFacturacion;
        private readonly IGenericRepository<AjustesWeb> _repositoryAjustesWeb;

        public AjusteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repositoryAjustes = _unitOfWork.Repository<Ajustes>();
            _repositoryAjustesFacturacion = _unitOfWork.Repository<AjustesFacturacion>();
            _repositoryAjustesWeb = _unitOfWork.Repository<AjustesWeb>();
        }

        public async Task<AjustesFacturacion> GetAjustesFacturacion(int idTienda)
        {
            var ajustes = await _repositoryAjustesFacturacion.GetAsNoTracking(_ => _.IdTienda == idTienda);
            try
            {

                ajustes.CertificadoPassword = !string.IsNullOrEmpty(ajustes.CertificadoPassword) ? EncryptionHelper.DecryptString(ajustes.CertificadoPassword) : null;
            }
            catch (ArgumentException e)
            {
            }
            catch (Exception e)
            {
                throw;
            }
            return ajustes;
        }

        public async Task<AjustesWeb> GetAjustesWeb()
        {
            return await _repositoryAjustesWeb.First();
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
            Ajustes_found.CompraMinima = entity.CompraMinima;
            Ajustes_found.CostoEnvio = entity.CostoEnvio;
            Ajustes_found.HabilitarWeb = entity.HabilitarWeb;
            Ajustes_found.TakeAwayDescuento = entity.TakeAwayDescuento;
            Ajustes_found.HabilitarTakeAway = entity.HabilitarTakeAway;

            Ajustes_found.NombreComodin1 = entity.NombreComodin1;
            Ajustes_found.NombreComodin2 = entity.NombreComodin2;
            Ajustes_found.NombreComodin3 = entity.NombreComodin3;
            Ajustes_found.HabilitarComodin1 = entity.HabilitarComodin1;
            Ajustes_found.HabilitarComodin2 = entity.HabilitarComodin2;
            Ajustes_found.HabilitarComodin3 = entity.HabilitarComodin3;

            var webRepository = _unitOfWork.Repository<AjustesWeb>();
            bool response = await webRepository.Edit(Ajustes_found);

            if (!response)
                throw new TaskCanceledException("Ajustes Web no se pudo cambiar.");

            return Ajustes_found;
        }

        public async Task<Ajustes> GetAjustes(int idTienda)
        {
            var ajustes = await _repositoryAjustes.GetAsNoTracking(_ => _.IdTienda == idTienda);
            try
            {
                ajustes.PasswordEmailEmisorCierreTurno = !string.IsNullOrEmpty(ajustes.PasswordEmailEmisorCierreTurno) ? EncryptionHelper.DecryptString(ajustes.PasswordEmailEmisorCierreTurno) : null;
            }
            catch (ArgumentException e)
            {
            }
            catch (Exception e)
            {
                throw;
            }
            return ajustes;
        }

        public async Task<Ajustes> Edit(Ajustes entity)
        {
            var Ajustes_found = await _repositoryAjustes.First(_ => _.IdTienda == entity.IdTienda);

            Ajustes_found.ModificationDate = TimeHelper.GetArgentinaTime();
            Ajustes_found.ModificationUser = entity.ModificationUser;

            Ajustes_found.EmailEmisorCierreTurno = entity.EmailEmisorCierreTurno;
            Ajustes_found.PasswordEmailEmisorCierreTurno = !string.IsNullOrEmpty(entity.PasswordEmailEmisorCierreTurno) ? EncryptionHelper.EncryptString(entity.PasswordEmailEmisorCierreTurno) : null;
            Ajustes_found.EmailsReceptoresCierreTurno = entity.EmailsReceptoresCierreTurno; 
            Ajustes_found.NotificarEmailCierreTurno = entity.NotificarEmailCierreTurno;
            Ajustes_found.ControlTotalesCierreTurno = entity.ControlTotalesCierreTurno;
            Ajustes_found.CodigoSeguridad = entity.CodigoSeguridad;
            Ajustes_found.ImprimirDefault = entity.ImprimirDefault;
            Ajustes_found.FacturaElectronica = entity.FacturaElectronica;
            Ajustes_found.ControlStock = entity.ControlStock;
            Ajustes_found.Encabezado1 = entity.Encabezado1;
            Ajustes_found.Encabezado2 = entity.Encabezado2;
            Ajustes_found.Encabezado3 = entity.Encabezado3;
            Ajustes_found.Pie1 = entity.Pie1;
            Ajustes_found.Pie2 = entity.Pie2;
            Ajustes_found.Pie3 = entity.Pie3;
            Ajustes_found.NombreImpresora = entity.NombreImpresora;
            Ajustes_found.MinimoIdentificarConsumidor = entity.MinimoIdentificarConsumidor;
            Ajustes_found.ControlEmpleado = entity.ControlEmpleado;

            bool response = await _repositoryAjustes.Edit(Ajustes_found);

            if (!response)
                throw new TaskCanceledException("Ajustes no se pudo cambiar.");

            return Ajustes_found;
        }
        public async Task<AjustesFacturacion> EditFacturacion(AjustesFacturacion entity)
        {
            var Ajustes_found = await _repositoryAjustesFacturacion.First(_ => _.IdTienda == entity.IdTienda);

            Ajustes_found.IsProdEnvironment = entity.IsProdEnvironment;
            Ajustes_found.PuntoVenta = entity.PuntoVenta;
            Ajustes_found.CondicionIva = entity.CondicionIva;
            Ajustes_found.NombreTitular = entity.NombreTitular;
            Ajustes_found.IngresosBurutosNro = entity.IngresosBurutosNro;
            Ajustes_found.FechaInicioActividad = entity.FechaInicioActividad;
            Ajustes_found.DireccionFacturacion = entity.DireccionFacturacion;
            Ajustes_found.CertificadoPassword = !string.IsNullOrEmpty(entity.CertificadoPassword) ? EncryptionHelper.EncryptString(entity.CertificadoPassword) : null;

            Ajustes_found.Cuit = entity.Cuit != 0 ? entity.Cuit : Ajustes_found.Cuit;
            Ajustes_found.CertificadoFechaCaducidad = entity.CertificadoFechaCaducidad != null ? entity.CertificadoFechaCaducidad : Ajustes_found.CertificadoFechaCaducidad;
            Ajustes_found.CertificadoFechaInicio = entity.CertificadoFechaInicio != null ? entity.CertificadoFechaInicio : Ajustes_found.CertificadoFechaInicio;
            Ajustes_found.CertificadoNombre = !string.IsNullOrEmpty(entity.CertificadoNombre) ? entity.CertificadoNombre : Ajustes_found.CertificadoNombre;

            Ajustes_found.ModificationDate = TimeHelper.GetArgentinaTime();
            Ajustes_found.ModificationUser = entity.ModificationUser;

            bool response = await _repositoryAjustesFacturacion.Edit(Ajustes_found);

            if (!response)
                throw new TaskCanceledException("Ajustes Facturacion no se pudo cambiar.");

            return Ajustes_found;
        }

        public async Task CreateAjsutes(int idTienda)
        {
            var ajustes = new Ajustes
            {
                IdTienda = idTienda,
                MinimoIdentificarConsumidor = 300000

            };
            var ajustesFacturacion = new AjustesFacturacion()
            {
                IdTienda = idTienda,
                IsProdEnvironment = false
            };
            await _repositoryAjustes.Add(ajustes);
            await _repositoryAjustesFacturacion.Add(ajustesFacturacion);
        }
    }
}
