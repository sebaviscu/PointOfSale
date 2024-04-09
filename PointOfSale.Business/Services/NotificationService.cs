using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;
using PointOfSale.Model;

namespace PointOfSale.Business.Services
{
    public class NotificationService : INotificationService
    {
        public DateTime DateTimeNowArg = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));
        private readonly IGenericRepository<Notifications> _repository;

        public NotificationService(IGenericRepository<Notifications> genericRepository)
        {
            _repository = genericRepository;
        }

        public async Task<List<Notifications>> List()
        {
            IQueryable<Notifications> query = await _repository.Query();
            return query.ToList();
        }

        public async Task<List<Notifications>> GetActive()
        {
            IQueryable<Notifications> query = await _repository.Query(_ => _.IsActive);
            return query.ToList();
        }

        public async Task<Notifications> Save(Notifications notifications)
        {
            try
            {
                notifications.RegistrationDate = DateTimeNowArg;
                var not = await _repository.Add(notifications);
                if (not.IdNotifications == 0)
                    throw new TaskCanceledException("La Notificacion no se pudo crear.");

                return not;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<Notifications> Edit(int idNotificacion, string modificationUser)
        {
            Notifications Notifications_found = await _repository.Get(c => c.IdNotifications == idNotificacion);
            Notifications_found.ModificationDate = DateTimeNowArg;
            Notifications_found.ModificationUser = modificationUser;
            Notifications_found.IsActive = false;

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
                Notifications_found.ModificationDate = DateTimeNowArg;
                Notifications_found.ModificationUser = modificationUser;
                Notifications_found.IsActive = false;

                var resp = await _repository.Edit(Notifications_found);
                if (!resp)
                    throw new TaskCanceledException("La Notificacion " + Notifications_found.IdNotifications + " no se pudo modificar.");
                return false;
            }

            return true;
        }
    }
}
