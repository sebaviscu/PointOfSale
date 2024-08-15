using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface IAjusteService
    {
        Task<AjustesWeb> GetAjustesWeb();

        Task<AjustesWeb> EditWeb(AjustesWeb entity);

        Task<Ajustes> GetAjustes(int idTienda);

        Task<Ajustes> Edit(Ajustes entity);

        Task<AjustesFacturacion> GetAjustesFacturacion(int idTienda);
        Task<AjustesFacturacion> EditFacturacion(AjustesFacturacion entity);

        Task CreateAjsutes(int idTienda);
        
        Task<AjustesFacturacion> UpdateCertificateInfo(AjustesFacturacion entity);

    }
}
