using System.ServiceModel.Channels;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.Repository;
using PointOfSale.Model;

namespace PointOfSale.Business.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IGenericRepository<Notifications> _repository;

        public NotificationService(IGenericRepository<Notifications> genericRepository)
        {
            _repository = genericRepository;
        }

        public async Task<List<Notifications>> List(int idTienda)
        {
            IQueryable<Notifications> query = await _repository.Query(_=> !_.IdTienda.HasValue || _.IdTienda == idTienda);
            return query.ToList();
        }

        public async Task<List<Notifications>> GetActive(int idTienda)
        {
            IQueryable<Notifications> query = await _repository.Query(_ => _.IsActive && (!_.IdTienda.HasValue || _.IdTienda == idTienda));
            return query.ToList();
        }
        public async Task<List<Notifications>> GetByUserByActive(int idUser)
        {
            IQueryable<Notifications> query = await _repository.Query(_ => _.IdUser == idUser && _.IsActive);
            return query.ToList();
        }

        public async Task<Notifications> Save(Notifications notifications)
        {
            notifications.RegistrationDate = TimeHelper.GetArgentinaTime();
            var not = await _repository.Add(notifications);
            if (not.IdNotifications == 0)
                throw new TaskCanceledException("La Notificacion no se pudo crear.");

            return not;
        }

        public async Task<List<Notifications>> SaveRange(List<Notifications> notificationsList)
        {
            var not = await _repository.AddRange(notificationsList);
            if (not.Count == 0)
                throw new TaskCanceledException("Las Notificaciones no se pudieron crear.");

            return not;
        }

        public async Task<Notifications> RecibirNotificacion(int idNotificacion, string modificationUser)
        {
            Notifications Notifications_found = await _repository.Get(c => c.IdNotifications == idNotificacion);
            Notifications_found.ModificationDate = TimeHelper.GetArgentinaTime();
            Notifications_found.ModificationUser = modificationUser;
            Notifications_found.IsActive = false;

            var resp = await _repository.Edit(Notifications_found);
            if (!resp)
                throw new TaskCanceledException("La Notificacion no se pudo modificar.");

            return Notifications_found;
        }

        public async Task<Notifications> Edit(Notifications model)
        {
            Notifications Notifications_found = await _repository.Get(c => c.IdNotifications == model.IdNotifications);

            var nombreUser = Notifications_found.Descripcion.Split("</strong>");

            Notifications_found.Descripcion = $"{nombreUser[0]} </strong>{model.Descripcion}."; ;

            var resp = await _repository.Edit(Notifications_found);
            if (!resp)
                throw new TaskCanceledException("La Notificacion no se pudo modificar.");

            return Notifications_found;
        }

        public async Task<bool> LimpiarTodo(string modificationUser)
        {
            var NotificationsAll = await _repository.Query(c => c.IsActive);
            foreach (var Notifications_found in NotificationsAll.ToList())
            {
                Notifications_found.ModificationDate = TimeHelper.GetArgentinaTime();
                Notifications_found.ModificationUser = modificationUser;
                Notifications_found.IsActive = false;

                var resp = await _repository.Edit(Notifications_found);
                if (!resp)
                    throw new TaskCanceledException("La Notificacion " + Notifications_found.IdNotifications + " no se pudo modificar.");

            }

            return true;
        }
        public async Task<bool> LimpiarIndividuales(string modificationUser, int idUser)
        {
            var NotificationsAll = await _repository.Query(c => c.IsActive && c.IdUser == idUser);
            foreach (var Notifications_found in NotificationsAll.ToList())
            {
                Notifications_found.ModificationDate = TimeHelper.GetArgentinaTime();
                Notifications_found.ModificationUser = modificationUser;
                Notifications_found.IsActive = false;

                var resp = await _repository.Edit(Notifications_found);
                if (!resp)
                    throw new TaskCanceledException("La Notificacion " + Notifications_found.IdNotifications + " no se pudo modificar.");

            }

            return true;
        }
        public async Task<bool> Delete(int idNotificacion)
        {
            var Notifications_found = await _repository.Get(c => c.IdNotifications == idNotificacion);

            if (Notifications_found == null)
                throw new TaskCanceledException("Notificacion no existe");


            return await _repository.Delete(Notifications_found);
        }
    }
}
