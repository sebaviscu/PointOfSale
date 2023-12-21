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
        Task<List<Notifications>> List();
        Task<List<Notifications>> GetActive();
        Task<Notifications> Save(Notifications notifications);
        Task<Notifications> Edit(int idNotificacion, string modificationUser);
        Task<bool> LimpiarTodo(string modificationUser);
    }
}
