using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface INotificationService
    {
        Task<List<Notifications>> List(int idTienda);
        Task<List<Notifications>> GetActive(int idTienda);
        Task<Notifications> Save(Notifications notifications);
        Task<Notifications> RecibirNotificacion(int idNotificacion, string modificationUser);
        Task<bool> LimpiarTodo(string modificationUser);

        Task<List<Notifications>> GetByUserByActive(int idUser);

        Task<bool> LimpiarIndividuales(string modificationUser, int idUser);

        Task<Notifications> Edit(Notifications model);

        Task<bool> Delete(int idNotificacion);

        Task<List<Notifications>> SaveRange(List<Notifications> notificationsList);
    }
}
